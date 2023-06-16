using Path.Entities;
using Path.Entities.Pedestrian;
using Rafael.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Path.AI.Pedestrian
{
    public class PedestrianPathFinding
    {
        public static List<Vector3> GetPathBetween(NodeObject startNode, NodeObject endNode)
        {
            List<PedestrianPathNode> pathNodesForPath = AStarSearch(startNode, endNode);
            List<Vector3> path = new();
            int numPathPoints = 15;
            for (int i = 0; i < pathNodesForPath.Count - 1; i++)
            {
                RafaelUtils.LineLineIntersection(
                    out Vector3 intersection,
                    pathNodesForPath[i].Position,
                    pathNodesForPath[i].Direction,
                    pathNodesForPath[i + 1].Position,
                    pathNodesForPath[i + 1].Direction);

                for (int j = 0; j < numPathPoints; j++)
                {
                    float t = j / (float)(numPathPoints - 1);
                    Vector3 pathPoint = Bezier.QuadraticCurve(
                        pathNodesForPath[i].Position,
                        pathNodesForPath[i + 1].Position,
                        intersection,
                        t);
                    path.Add(pathPoint);
                }
            }
            return path;
        }

        private static List<PedestrianPathNode> AStarSearch(NodeObject startNode, NodeObject endNode)
        {
            List<NodeObject> nodes = PathFinding.GetPathBetween(startNode, endNode);
            PathObject pathStart;
            PathObject pathEnd;

            PedestrianPathNode startPathNode;
            PedestrianPathNode endPathNode;
            List<PedestrianPathNode> path = new();

            List<PedestrianPathNode> nodesTocheck = new();
            Dictionary<PedestrianPathNode, float> costDictionary = new();
            Dictionary<PedestrianPathNode, float> priorityDictionary = new();
            Dictionary<PedestrianPathNode, PedestrianPathNode> parentsDictionary = new();

            try
            {
                pathStart = PathManager.Instance.GetPathBetween(nodes[0], nodes[1]);
                pathEnd = PathManager.Instance.GetPathBetween(nodes[nodes.Count - 2], nodes[nodes.Count - 1]);
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.LogError("Start and end have no connecting path");
                // NOTE: Start and end have no connecting path between then.
                return path;
            }

            try
            {
                if (startNode.IsStartNodeOf(pathStart))
                    startPathNode = startNode.GetPedestrianPathNodeFor(pathStart, PathNodeObject.OnPathPosition.StartNodeStartPath);
                else
                    startPathNode = startNode.GetPedestrianPathNodeFor(pathStart, PathNodeObject.OnPathPosition.EndNodeStartPath);

                if (endNode.IsStartNodeOf(pathEnd))
                    endPathNode = endNode.GetPedestrianPathNodeFor(pathEnd, PathNodeObject.OnPathPosition.StartNodeEndPath);
                else
                    endPathNode = endNode.GetPedestrianPathNodeFor(pathEnd, PathNodeObject.OnPathPosition.EndNodeEndPath);
            }
            catch(KeyNotFoundException)
            {
                Debug.LogError("Path not found!");
                // TODO: Handle when path not found!
                return path;
            }

            nodesTocheck.Add(startPathNode);
            priorityDictionary.Add(startPathNode, 0);
            costDictionary.Add(startPathNode, 0);
            parentsDictionary.Add(startPathNode, null);

            while (nodesTocheck.Count > 0)
            {
                PedestrianPathNode currentNode = GetClosestPathNode(nodesTocheck, priorityDictionary);
                nodesTocheck.Remove(currentNode);
                if (currentNode.Equals(endPathNode))
                {
                    path = GeneratePath(parentsDictionary, currentNode);
                    return path;
                }

                foreach (PedestrianPathNode neighbour in currentNode.GetConnectedNodes())
                {
                    float newCost = costDictionary[currentNode] + 1;
                    if (!costDictionary.ContainsKey(neighbour) || newCost < costDictionary[neighbour])
                    {
                        costDictionary[neighbour] = newCost;

                        float priority = newCost + ManhattanDiscance(endPathNode, neighbour);
                        nodesTocheck.Add(neighbour);
                        priorityDictionary[neighbour] = priority;

                        parentsDictionary[neighbour] = currentNode;
                    }
                }
            }
            return path;
        }

        private static PedestrianPathNode GetClosestPathNode(List<PedestrianPathNode> list, Dictionary<PedestrianPathNode, float> distanceMap)
        {
            PedestrianPathNode candidate = list[0];
            foreach (PedestrianPathNode vertex in list)
            {
                if (distanceMap[vertex] < distanceMap[candidate])
                {
                    candidate = vertex;
                }
            }
            return candidate;
        }

        private static float ManhattanDiscance(PedestrianPathNode endPos, PedestrianPathNode position)
        {
            return Math.Abs(endPos.Position.x - position.Position.x) + Math.Abs(endPos.Position.z - position.Position.z);
        }

        public static List<PedestrianPathNode> GeneratePath(Dictionary<PedestrianPathNode, PedestrianPathNode> parentMap, PedestrianPathNode endState)
        {
            List<PedestrianPathNode> path = new();
            PedestrianPathNode parent = endState;
            while (parent != null && parentMap.ContainsKey(parent))
            {
                path.Add(parent);
                parent = parentMap[parent];
            }
            path.Reverse();
            return path;
        }
    }
}