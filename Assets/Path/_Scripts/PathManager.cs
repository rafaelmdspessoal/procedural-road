using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Path.Entities;
using Path.PlacementSystem;

namespace Path {

    public class PathManager : MonoBehaviour {
        public static PathManager Instance { get; private set; }

        private PathPlacementSystem pathPlacementSystem;
        private readonly Dictionary<Vector3, NodeObject> placedNodesDict = new();

        [SerializeField] private Transform pathParentTransform;
        [SerializeField] private Transform nodeParentTransform;

        public bool updatePaths = false;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            pathPlacementSystem = PathPlacementSystem.Instance;
            pathPlacementSystem.OnPathPlaced += PathPlacementSystem_OnPathPlaced;
        }
        public void AddNode(NodeObject node) 
        {
            if (!HasNode(node))
                placedNodesDict.Add(node.Position, node);
        }
        private void PathPlacementSystem_OnPathPlaced(object sender, PathPlacementSystem.OnPathPlacedEventArgs e)
        {
            PathObject pathObject = e.pathObject;
            pathObject.OnPathRemoved += PathObject_OnPathRemoved;
            pathObject.OnPathUpdated += PathObject_OnPathUpdated;
            pathObject.OnPathBuilt += PathObject_OnPathBuilt;
        }
        private void PathObject_OnPathBuilt(object sender, EventArgs e) {
            throw new NotImplementedException();
        }
        private void PathObject_OnPathUpdated(object sender, EventArgs e) {
            throw new NotImplementedException();
        }
        private void PathObject_OnPathRemoved(object sender, EventArgs e) {
            //PathObject pathObject = (PathObject)sender;
            //if (pathObject.StartNode != null && !pathObject.StartNode.HasConnectedPaths) RemoveNode(pathObject.StartNode);
            //if (pathObject.EndNode != null && !pathObject.EndNode.HasConnectedPaths) RemoveNode(pathObject.EndNode);
        }
        public void RemoveNode(NodeObject node)
        {
            if (HasNode(node))
            {
                placedNodesDict.Remove(node.Position);
                Destroy(node.gameObject);
            }
        }
        private bool HasNode(NodeObject node) => placedNodesDict.ContainsValue(node);
        public bool HasNode(Vector3 position) => placedNodesDict.ContainsKey(position);
        public Transform PathParentTransform => pathParentTransform;
        public NodeObject GetNodeAt(Vector3 position) => placedNodesDict.GetValueOrDefault(position);
        public NodeObject GetOrCreateNodeAt(Vector3 position, PathObject pathObject) {
            if (HasNode(position)) {
                NodeObject existingNode = GetNodeAt(position);
                return existingNode;
            }

            GameObject nodeObject = Instantiate(pathObject.NodePrefab, position, Quaternion.identity, nodeParentTransform);
            NodeObject newNode = nodeObject.GetComponent<NodeObject>();
            AddNode(newNode);
            return newNode;
        }
        public NodeObject GetRandomNode(NodeObject.PathFor pathFor)
        {
            int nodeIndex = 0;
            List<NodeObject> pathNodes = placedNodesDict.Values.Where(x => x.PathEntity == pathFor).ToList();

            for (int i = 0; i < pathNodes.Count; i++)
            {
                nodeIndex = UnityEngine.Random.Range(0, pathNodes.Count);
                if (nodeIndex > 0) break;
            }
            return pathNodes[nodeIndex];
        }
        public PathObject GetPathBetween(NodeObject startNode, NodeObject endNode)
        {
            foreach (PathObject pathObject in startNode.ConnectedPaths)
            {
                if (pathObject.OtherNodeTo(startNode).Equals(endNode))
                    return pathObject;
            }
            return null;
        }
    }
}