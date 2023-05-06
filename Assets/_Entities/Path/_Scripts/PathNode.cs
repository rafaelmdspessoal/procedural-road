using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rafael.Utils;
using System;
using Roads;

public class PathNode : MonoBehaviour, IEquatable<PathNode>
{
    public enum PathOrientation
    {
        Start,
        End,
    }


    public EventHandler OnConnectionRemoved;

    [SerializeField] private PathOrientation pathOrientation;
    [SerializeField] private List<PathNode> connectedNodesList = new();

    public void Init(PathOrientation pathOrientation, string name)
    {
        this.pathOrientation = pathOrientation;
        transform.name = name;
    }

    public void AddPathNode(PathNode pathNode)
    {
        connectedNodesList.Add(pathNode);
        pathNode.OnConnectionRemoved += PathNode_OnConnectionRemoved;
    }

    private void PathNode_OnConnectionRemoved(object sender, EventArgs e)
    {
        PathNode pathNode = (PathNode)sender;
        pathNode.OnConnectionRemoved -= PathNode_OnConnectionRemoved;
        pathNode.RemovePathConnection(this);
        connectedNodesList.Remove(pathNode);
        Destroy(pathNode.gameObject);
    }

    public void RemovePathConnection(PathNode pathNode)
    {
        connectedNodesList.Remove(pathNode);
        OnConnectionRemoved?.Invoke(this, EventArgs.Empty);
        OnConnectionRemoved = null;
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

    public List<PathNode> getConnectedNodes()
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
