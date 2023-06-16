using System.Collections.Generic;
using UnityEngine;
using System;

namespace Path.Entities
{
    public abstract class PathNodeObject : MonoBehaviour, IEquatable<PathNodeObject>
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
        [SerializeField] private List<PathNodeObject> connectedNodesList = new();

        public OnPathPosition PathPosition => pathPosition;
        public Vector3 Position => transform.position;
        public Vector3 Direction => transform.forward;

        public void AddPathNode(PathNodeObject pathNode)
        {
            if (!connectedNodesList.Contains(pathNode))
            {
                connectedNodesList.Add(pathNode);
                pathNode.OnConnectionRemoved += PathNode_OnConnectionRemoved;
            }
        }

        public void ClearConnections()
        {
            foreach (PathNodeObject pathNode in connectedNodesList)
            {
                OnConnectionRemoved?.Invoke(this, EventArgs.Empty);
                OnConnectionRemoved = null;
            }
            connectedNodesList.Clear();
        }

        public bool Equals(PathNodeObject other)
        {
            return Vector3.SqrMagnitude(Position - other.Position) < 0.0001f;
        }
        public List<PathNodeObject> GetConnectedNodes()
        {
            return connectedNodesList;
        }

        public virtual void Init(OnPathPosition pathPosition)
        {
            this.pathPosition = pathPosition;
        }

        public bool IsStartOfPath => (
            pathPosition == OnPathPosition.StartNodeStartPath ||
            pathPosition == OnPathPosition.EndNodeStartPath
            );

        private void PathNode_OnConnectionRemoved(object sender, EventArgs e)
        {
            PathNodeObject pathNode = (PathNodeObject)sender;
            RemovePathConnection(pathNode);
        }

        public void RemovePathConnection(PathNodeObject pathNode)
        {
            connectedNodesList.Remove(pathNode);
        }

        protected virtual void OnDestroy()
        {
            OnConnectionRemoved?.Invoke(this, EventArgs.Empty);
            OnConnectionRemoved = null;
        }
    }
}
