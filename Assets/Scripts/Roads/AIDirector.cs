using Nodes;
using Roads.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDirector : MonoBehaviour
{
    private RoadManager roadManager;
    private List<Vector3> path = new();

    public LineRenderer lineRenderer;

    private void Start()
    {
        roadManager = RoadManager.Instance;
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void GetPathBetween(Node startNode, Node endNode)
    {
        path = PedestrianPathFinding.GetPathBetween(startNode, endNode);

        if (path.Count > 1)
        {
            lineRenderer.positionCount = path.Count;
            Vector3[] positions = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                positions[i] = path[i] + Vector3.up * 0.1f;
            }
            lineRenderer.SetPositions(positions);
        }
    }

    public Node GetRandomNode()
    {
        return roadManager.GetRandomNode();
    }
}
