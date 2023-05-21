using Path.Entities;
using System.Collections.Generic;
using UnityEngine;
using Path.AI.Pedestrian;

namespace Path.AI
{
    public class AIDirector : MonoBehaviour
    {
        private PathManager pathManager;
        [SerializeReference] private NodeObject.PathFor pathFor;

        private List<Vector3> path = new();

        public LineRenderer lineRenderer;

        private void Start()
        {
            pathManager = PathManager.Instance;
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void GetPathBetween(NodeObject startNode, NodeObject endNode)
        {
            if (pathFor == NodeObject.PathFor.Pedestrian) 
                path = PedestrianPathFinding.GetPathBetween(startNode, endNode);
            else
                path = VehiclePathFinding.GetPathBetween(startNode, endNode);

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

        public NodeObject GetRandomNode()
        {
            return pathManager.GetRandomNode(pathFor);
        }
    }
}
