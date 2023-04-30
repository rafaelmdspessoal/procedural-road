using Nodes;
using Roads.Manager;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PathFinding
{
    public static List<Node> GetPathBetween(Node startNode, Node endNode)
    {
        List<Node> resultPath = AStarSearch(startNode, endNode);
        List<Node> path = new();
        foreach (Node node in resultPath)
        {
            path.Add(node);
        }
        return path;
    }

    private static List<Node> AStarSearch(Node startNode, Node endNode)
    {
        List<Node> path = new();

        List<Node> positionsTocheck = new();
        Dictionary<Node, float> costDictionary = new();
        Dictionary<Node, float> priorityDictionary = new();
        Dictionary<Node, Node> parentsDictionary = new();

        positionsTocheck.Add(startNode);
        priorityDictionary.Add(startNode, 0);
        costDictionary.Add(startNode, 0);
        parentsDictionary.Add(startNode, null);

        while (positionsTocheck.Count > 0)
        {
            Node current = GetClosestNode(positionsTocheck, priorityDictionary);
            positionsTocheck.Remove(current);
            if (current.Equals(endNode))
            {
                path = GeneratePath(parentsDictionary, current);
                return path;
            }

            foreach (Node neighbour in current.GetConnectedNodes())
            {
                float newCost = costDictionary[current] + 1;
                if (!costDictionary.ContainsKey(neighbour) || newCost < costDictionary[neighbour])
                {
                    costDictionary[neighbour] = newCost;

                    float priority = newCost + ManhattanDiscance(endNode, neighbour);
                    positionsTocheck.Add(neighbour);
                    priorityDictionary[neighbour] = priority;

                    parentsDictionary[neighbour] = current;
                }
            }
        }
        return path;
    }

    private static Node GetClosestNode(List<Node> list, Dictionary<Node, float> distanceMap)
    {
        Node candidate = list[0];
        foreach (Node vertex in list)
        {
            if (distanceMap[vertex] < distanceMap[candidate])
            {
                candidate = vertex;
            }
        }
        return candidate;
    }

    private static float ManhattanDiscance(Node endPos, Node position)
    {
        return Math.Abs(endPos.Position.x - position.Position.x) + Math.Abs(endPos.Position.z - position.Position.z);
    }

    public static List<Node> GeneratePath(Dictionary<Node, Node> parentMap, Node endState)
    {
        List<Node> path = new();
        Node parent = endState;
        while (parent != null && parentMap.ContainsKey(parent))
        {
            path.Add(parent);
            parent = parentMap[parent];
        }
        path.Reverse();
        return path;
    }
}
