using System.Collections.Generic;
using UnityEngine;
using System;

namespace Path.Entities.Pedestrian
{
    public class PedestrianPathNode : MonoBehaviour, IEquatable<PedestrianPathNode>
    {
        public enum OnPathPosition
        {
            StartNodeStartPath,
            StartNodeEndPath,
            EndNodeStartPath,
            EndNodeEndPath,
        }

        public EventHandler OnConnectionRemoved;

        [SerializeField] private OnPathPosition pathPosition;
        [SerializeField] private List<PedestrianPathNode> connectedNodesList = new();

        public OnPathPosition PathPosition => pathPosition;
        public Vector3 Position => transform.position;
        public Vector3 Direction => transform.forward;

        public void AddPathNode(PedestrianPathNode pathNode)
        {
            connectedNodesList.Add(pathNode);
            pathNode.OnConnectionRemoved += PathNode_OnConnectionRemoved;
        }

        public void ClearConnections()
        {
            foreach (PedestrianPathNode pathNode in connectedNodesList)
            {
                OnConnectionRemoved?.Invoke(this, EventArgs.Empty);
                OnConnectionRemoved = null;
            }
            connectedNodesList.Clear();
        }

        public bool Equals(PedestrianPathNode other)
        {
            return Vector3.SqrMagnitude(Position - other.Position) < 0.0001f;
        }
        public List<PedestrianPathNode> GetConnectedNodes()
        {
            return connectedNodesList;
        }

        public void Init(OnPathPosition pathPosition)
        {
            this.pathPosition = pathPosition;
            transform.name = pathPosition.ToString();
        }

        public bool IsStartOfPath => (
            pathPosition == OnPathPosition.StartNodeStartPath ||
            pathPosition == OnPathPosition.EndNodeStartPath
            );

        private void PathNode_OnConnectionRemoved(object sender, EventArgs e)
        {
            PedestrianPathNode pathNode = (PedestrianPathNode)sender;
            RemovePathConnection(pathNode);
        }

        public void RemovePathConnection(PedestrianPathNode pathNode)
        {
            connectedNodesList.Remove(pathNode);
        }

        private void OnDestroy()
        {
            OnConnectionRemoved?.Invoke(this, EventArgs.Empty);
            OnConnectionRemoved = null;
        }
    }
}
