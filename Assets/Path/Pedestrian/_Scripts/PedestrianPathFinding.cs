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
            List<PathNodeObject> pathNodesForPath = AStarSearch(startNode, endNode);
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

        private static List<PathNodeObject> AStarSearch(NodeObject startNode, NodeObject endNode)
        {
            List<NodeObject> nodes = PathFinding.GetPathBetween(startNode, endNode);
            PathObject pathStart = PathManager.Instance.GetPathBetween(nodes[0], nodes[1]);
            PathObject pathEnd = PathManager.Instance.GetPathBetween(nodes[nodes.Count - 2], nodes[nodes.Count - 1]);
            PathNodeObject startPathNode;
            PathNodeObject endPathNode;
            List<PathNodeObject> path = new();

            if (startNode.IsStartNodeOf(pathStart))
                startPathNode = startNode.GetPathNodeFor(pathStart, PathNodeObject.OnPathPosition.StartNodeStartPath);
            else
                startPathNode = startNode.GetPathNodeFor(pathStart, PathNodeObject.OnPathPosition.EndNodeStartPath);

            if (endNode.IsStartNodeOf(pathEnd))
                endPathNode = endNode.GetPathNodeFor(pathEnd, PathNodeObject.OnPathPosition.StartNodeEndPath);
            else
                endPathNode = endNode.GetPathNodeFor(pathEnd, PathNodeObject.OnPathPosition.EndNodeEndPath);

            List<PathNodeObject> nodesTocheck = new();
            Dictionary<PathNodeObject, float> costDictionary = new();
            Dictionary<PathNodeObject, float> priorityDictionary = new();
            Dictionary<PathNodeObject, PathNodeObject> parentsDictionary = new();

            nodesTocheck.Add(startPathNode);
            priorityDictionary.Add(startPathNode, 0);
            costDictionary.Add(startPathNode, 0);
            parentsDictionary.Add(startPathNode, null);

            while (nodesTocheck.Count > 0)
            {
                PathNodeObject currentNode = GetClosestPathNode(nodesTocheck, priorityDictionary);
                nodesTocheck.Remove(currentNode);
                if (currentNode.Equals(endPathNode))
                {
                    path = GeneratePath(parentsDictionary, currentNode);
                    return path;
                }

                foreach (PathNodeObject neighbour in currentNode.GetConnectedNodes())
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

        private static PathNodeObject GetClosestPathNode(List<PathNodeObject> list, Dictionary<PathNodeObject, float> distanceMap)
        {
            PathNodeObject candidate = list[0];
            foreach (PathNodeObject vertex in list)
            {
                if (distanceMap[vertex] < distanceMap[candidate])
                {
                    candidate = vertex;
                }
            }
            return candidate;
        }

        private static float ManhattanDiscance(PathNodeObject endPos, PathNodeObject position)
        {
            return Math.Abs(endPos.Position.x - position.Position.x) + Math.Abs(endPos.Position.z - position.Position.z);
        }

        public static List<PathNodeObject> GeneratePath(Dictionary<PathNodeObject, PathNodeObject> parentMap, PathNodeObject endState)
        {
            List<PathNodeObject> path = new();
            PathNodeObject parent = endState;
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