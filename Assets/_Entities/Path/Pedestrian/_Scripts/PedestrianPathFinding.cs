using Nodes;
using Rafael.Utils;
using Roads;
using Roads.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianPathFinding
{    public static List<Vector3> GetPathBetween(Node startNode, Node endNode)
    {
        List<PathNode> pathNodesForPath = AStarSearch(startNode, endNode);
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

    private static List<PathNode> AStarSearch(Node startNode, Node endNode)
    {
        List<Node> nodes = PathFinding.GetPathBetween(startNode, endNode);
        RoadObject startRoad = RoadManager.Instance.GetRoadBetween(nodes[0], nodes[1]);
        RoadObject endRoad = RoadManager.Instance.GetRoadBetween(nodes[nodes.Count - 2], nodes[nodes.Count - 1]);
        PathNode startPathNode;
        PathNode endPathNode;
        List <PathNode> path = new();

        if(startNode.IsStartNodeOf(startRoad)) 
            startPathNode = startNode.GetPathNodeFor(startRoad, PathNode.PathPosition.StartNodeStartPath);        
        else
            startPathNode = startNode.GetPathNodeFor(startRoad, PathNode.PathPosition.EndNodeStartPath);

        if (endNode.IsStartNodeOf(endRoad))
            endPathNode = endNode.GetPathNodeFor(endRoad, PathNode.PathPosition.StartNodeEndPath);
        else
            endPathNode = endNode.GetPathNodeFor(endRoad, PathNode.PathPosition.EndNodeEndPath);

        List<PathNode> nodesTocheck = new();
        Dictionary<PathNode, float> costDictionary = new();
        Dictionary<PathNode, float> priorityDictionary = new();
        Dictionary<PathNode, PathNode> parentsDictionary = new();

        nodesTocheck.Add(startPathNode);
        priorityDictionary.Add(startPathNode, 0);
        costDictionary.Add(startPathNode, 0);
        parentsDictionary.Add(startPathNode, null);

        while (nodesTocheck.Count > 0)
        {
            PathNode currentNode = GetClosestPathNode(nodesTocheck, priorityDictionary);
            nodesTocheck.Remove(currentNode);
            if (currentNode.Equals(endPathNode))
            {
                path = GeneratePath(parentsDictionary, currentNode);
                return path;
            }

            foreach (PathNode neighbour in currentNode.GetConnectedNodes())
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

    private static PathNode GetClosestPathNode(List<PathNode> list, Dictionary<PathNode, float> distanceMap)
    {
        PathNode candidate = list[0];
        foreach (PathNode vertex in list)
        {
            if (distanceMap[vertex] < distanceMap[candidate])
            {
                candidate = vertex;
            }
        }
        return candidate;
    }

    private static float ManhattanDiscance(PathNode endPos, PathNode position)
    {
        return Math.Abs(endPos.Position.x - position.Position.x) + Math.Abs(endPos.Position.z - position.Position.z);
    }

    public static List<PathNode> GeneratePath(Dictionary<PathNode, PathNode> parentMap, PathNode endState)
    {
        List<PathNode> path = new();
        PathNode parent = endState;
        while (parent != null && parentMap.ContainsKey(parent))
        {
            path.Add(parent);
            parent = parentMap[parent];
        }
        path.Reverse();
        return path;
    }
}
