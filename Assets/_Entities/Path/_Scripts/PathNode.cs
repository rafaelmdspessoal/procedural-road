using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rafael.Utils;
using System;
using Roads;

public class PathNode : MonoBehaviour, IEquatable<PathNode>
{
    public enum PathPosition
    {
        StartNodeStartPath,
        StartNodeEndPath,
        EndNodeStartPath,
        EndNodeEndPath,
    }


    public EventHandler OnConnectionRemoved;

    [SerializeField] private PathPosition pathPosition;
    [SerializeField] private List<PathNode> connectedNodesList = new();

    public PathPosition PathPos => pathPosition;

    public void Init(PathPosition pathPosition)
    {
        this.pathPosition = pathPosition;
        transform.name = pathPosition.ToString();
    }

    public bool IsStartOfPath => (
        pathPosition == PathPosition.StartNodeStartPath || 
        pathPosition == PathPosition.EndNodeStartPath
        );

    public void AddPathNode(PathNode pathNode)
    {
        connectedNodesList.Add(pathNode);
        pathNode.OnConnectionRemoved += PathNode_OnConnectionRemoved;
    }

    private void PathNode_OnConnectionRemoved(object sender, EventArgs e)
    {
        PathNode pathNode = (PathNode)sender;
        RemovePathConnection(pathNode);
    }

    private void OnDestroy()
    {
        OnConnectionRemoved?.Invoke(this, EventArgs.Empty);
        OnConnectionRemoved = null;
    }

    public void RemovePathConnection(PathNode pathNode)
    {
        connectedNodesList.Remove(pathNode);
    }

    public void ClearConnections()
    {
        foreach (PathNode pathNode in connectedNodesList)
        {
            OnConnectionRemoved?.Invoke(this, EventArgs.Empty);
            OnConnectionRemoved = null;
        }
        connectedNodesList.Clear();
    }

    public List<PathNode> GetConnectedNodes()
    {
        return connectedNodesList;
    }

    public Vector3 Position => transform.position;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var node in connectedNodesList)
        {
            Gizmos.DrawLine(Position, node.Position);
        }
    }

    public bool Equals(PathNode other)
    {
        return Vector3.SqrMagnitude(Position - other.Position) < 0.0001f;
    }
}
