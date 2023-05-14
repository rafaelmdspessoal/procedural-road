using Path.Entities;
using System;
using System.Collections.Generic;

namespace Path.AI
{
    public class PathFinding
    {
        public static List<NodeObject> GetPathBetween(NodeObject startNode, NodeObject endNode)
        {
            List<NodeObject> resultPath = AStarSearch(startNode, endNode);
            return resultPath;
        }

        private static List<NodeObject> AStarSearch(NodeObject startNode, NodeObject endNode)
        {
            List<NodeObject> path = new();

            List<NodeObject> nodesTocheck = new();
            Dictionary<NodeObject, float> costDictionary = new();
            Dictionary<NodeObject, float> priorityDictionary = new();
            Dictionary<NodeObject, NodeObject> parentsDictionary = new();

            nodesTocheck.Add(startNode);
            priorityDictionary.Add(startNode, 0);
            costDictionary.Add(startNode, 0);
            parentsDictionary.Add(startNode, null);

            while (nodesTocheck.Count > 0)
            {
                NodeObject currentNode = GetClosestNode(nodesTocheck, priorityDictionary);
                nodesTocheck.Remove(currentNode);
                if (currentNode.Equals(endNode))
                {
                    path = GeneratePath(parentsDictionary, currentNode);
                    return path;
                }

                foreach (NodeObject neighbour in currentNode.GetConnectedNodes())
                {
                    float newCost = costDictionary[currentNode] + 1;
                    if (!costDictionary.ContainsKey(neighbour) || newCost < costDictionary[neighbour])
                    {
                        costDictionary[neighbour] = newCost;

                        float priority = newCost + ManhattanDiscance(endNode, neighbour);
                        nodesTocheck.Add(neighbour);
                        priorityDictionary[neighbour] = priority;

                        parentsDictionary[neighbour] = currentNode;
                    }
                }
            }
            return path;
        }

        private static NodeObject GetClosestNode(List<NodeObject> list, Dictionary<NodeObject, float> distanceMap)
        {
            NodeObject candidate = list[0];
            foreach (NodeObject vertex in list)
            {
                if (distanceMap[vertex] < distanceMap[candidate])
                {
                    candidate = vertex;
                }
            }
            return candidate;
        }

        private static float ManhattanDiscance(NodeObject endPos, NodeObject position)
        {
            return Math.Abs(endPos.Position.x - position.Position.x) + Math.Abs(endPos.Position.z - position.Position.z);
        }

        public static List<NodeObject> GeneratePath(Dictionary<NodeObject, NodeObject> parentMap, NodeObject endState)
        {
            List<NodeObject> path = new();
            NodeObject parent = endState;
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