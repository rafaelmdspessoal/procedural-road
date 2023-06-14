using Path.Entities;
using Path.Entities.Vehicle;
using Rafael.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Path.AI.Pedestrian
{
    public class VehiclePathFinding
    {
        public static List<Vector3> GetPathBetween(NodeObject startNode, NodeObject endNode)
        {
            List<VehiclePathNode> pathNodesForPath = AStarSearch(startNode, endNode);
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

        private static List<VehiclePathNode> AStarSearch(NodeObject startNode, NodeObject endNode)
        {
            List<NodeObject> nodes = PathFinding.GetPathBetween(startNode, endNode);
            VehiclePath pathStart = PathManager.Instance.GetPathBetween(nodes[0], nodes[1]) as VehiclePath;
            VehiclePath pathEnd = PathManager.Instance.GetPathBetween(nodes[nodes.Count - 2], nodes[nodes.Count - 1]) as VehiclePath;
            VehiclePathNode startPathNode;
            VehiclePathNode endPathNode;
            List<VehiclePathNode> path = new();

            if (startNode.IsStartNodeOf(pathStart))
                startPathNode = startNode.GetVehiclePathNodeFor(pathStart, PathNodeObject.OnPathPosition.StartNodeStartPath);
            else
                startPathNode = startNode.GetVehiclePathNodeFor(pathStart, PathNodeObject.OnPathPosition.EndNodeStartPath);

            if (endNode.IsStartNodeOf(pathEnd))
                endPathNode = endNode.GetVehiclePathNodeFor(pathEnd, PathNodeObject.OnPathPosition.StartNodeEndPath);
            else
                endPathNode = endNode.GetVehiclePathNodeFor(pathEnd, PathNodeObject.OnPathPosition.EndNodeEndPath);

            List<VehiclePathNode> nodesTocheck = new();
            Dictionary<VehiclePathNode, float> costDictionary = new();
            Dictionary<VehiclePathNode, float> priorityDictionary = new();
            Dictionary<VehiclePathNode, VehiclePathNode> parentsDictionary = new();

            nodesTocheck.Add(startPathNode);
            priorityDictionary.Add(startPathNode, 0);
            costDictionary.Add(startPathNode, 0);
            parentsDictionary.Add(startPathNode, null);

            while (nodesTocheck.Count > 0)
            {
                VehiclePathNode currentNode = GetClosestPathNode(nodesTocheck, priorityDictionary);
                nodesTocheck.Remove(currentNode);
                if (currentNode.Equals(endPathNode))
                {
                    path = GeneratePath(parentsDictionary, currentNode);
                    return path;
                }

                foreach (VehiclePathNode neighbour in currentNode.GetConnectedNodes())
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

        private static VehiclePathNode GetClosestPathNode(List<VehiclePathNode> list, Dictionary<VehiclePathNode, float> distanceMap)
        {
            VehiclePathNode candidate = list[0];
            foreach (VehiclePathNode vertex in list)
            {
                if (distanceMap[vertex] < distanceMap[candidate])
                {
                    candidate = vertex;
                }
            }
            return candidate;
        }

        private static float ManhattanDiscance(VehiclePathNode endPos, VehiclePathNode position)
        {
            return Math.Abs(endPos.Position.x - position.Position.x) + Math.Abs(endPos.Position.z - position.Position.z);
        }

        public static List<VehiclePathNode> GeneratePath(Dictionary<VehiclePathNode, VehiclePathNode> parentMap, VehiclePathNode endState)
        {
            List<VehiclePathNode> path = new();
            VehiclePathNode parent = endState;
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