using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Path.Entities;
using Path.PlacementSystem;
using Path.Entities.Vehicle;
using Path.Entities.SO;
using UnityEditor;

namespace Path {

    public class PathManager : MonoBehaviour {
        struct SerializeablePath
        {
            public Vector3 startNodePosition;
            public Vector3 endNodePosition;
            public Vector3 controlPosition;
            public string pathSOName;

            public SerializeablePath(
                Vector3 startNodePosition,
                Vector3 endNodePosition,
                Vector3 controlPosition,
                string pathSOName)
            {
                this.startNodePosition = startNodePosition;
                this.endNodePosition = endNodePosition;
                this.controlPosition = controlPosition;
                this.pathSOName = pathSOName;
            }
        }
        public static PathManager Instance { get; private set; }

        private readonly Dictionary<Vector3, NodeObject> placedNodesDict = new();
        private List<SerializeablePath> pathObjects = new();

        [SerializeField] private Transform pathParentTransform;
        [SerializeField] private Transform nodeParentTransform;

        public bool updatePaths = false;
        private IDataService DataService = new JsonDataService();
        private bool encryptionEnabled;

        private void Awake() {
            Instance = this;
        }

        private void Start() {

        }
        public void ToggleEncryption(bool encryptionEnabled)
        {
            this.encryptionEnabled = encryptionEnabled;
        }
        public void LoadJson()
        {
            pathObjects = DataService.LoadData<List<SerializeablePath>>("/xuxa.json", encryptionEnabled);
            foreach (SerializeablePath pathObject in pathObjects)
            {
                Vector3 startNodePosition = pathObject.startNodePosition;
                Vector3 endNodePosition = pathObject.endNodePosition;
                Vector3 controlPosition = pathObject.controlPosition;
                PathSO pathSO = Resources.Load(pathObject.pathSOName) as PathSO;

                PathPlacementSystem.Instance.LoadRoadData(
                    startNodePosition,
                    endNodePosition,
                    controlPosition,
                    pathSO);
            }
        }
        public void SerializeJson()
        {
            if(DataService.SaveData("/xuxa.json", pathObjects, encryptionEnabled))
            {

            }
            else
            {
                Debug.LogError("Could not save file!");
            }
        }
        public void AddNode(NodeObject node) 
        {
            if (!HasNode(node))
                placedNodesDict.Add(node.Position, node);
        }
        public void AddPath(PathObject pathObject)
        {
            pathObject.OnPathRemoved += PathObject_OnPathRemoved;
            pathObject.OnPathUpdated += PathObject_OnPathUpdated;
            pathObject.OnPathBuilt += PathObject_OnPathBuilt;

            bool hasPath = pathObjects.Where(
                x => x.startNodePosition == pathObject.StartNode.Position
            ).Where(
                y => y.endNodePosition == pathObject.EndNode.Position).Count() > 0;
            if (!hasPath)
            {
                pathObjects.Add(new SerializeablePath(
                    pathObject.StartNode.Position,
                    pathObject.EndNode.Position,
                    pathObject.ControlPosition,
                    pathObject.PathSO.name));
            }
        }
        private void PathObject_OnPathBuilt(object sender, EventArgs e) {
            throw new NotImplementedException();
        }
        private void PathObject_OnPathUpdated(object sender, EventArgs e) {
            throw new NotImplementedException();
        }
        private void PathObject_OnPathRemoved(object sender, EventArgs e) {
            PathObject pathObject = (PathObject)sender;
            SerializeablePath path = pathObjects.Where(
               x => x.startNodePosition == pathObject.StartNode.Position
           ).Where(
               y => y.endNodePosition == pathObject.EndNode.Position).First();
            if (pathObjects.Contains(path))
            {
                pathObjects.Remove(path);
            }

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
            List<NodeObject> pathNodes;
            if (pathFor == NodeObject.PathFor.Vehicle)
                pathNodes = placedNodesDict.Values.Where(x => x.PathEntity == pathFor).ToList();
            else
            {
                pathNodes = placedNodesDict.Values.Where(x => x.PathEntity == pathFor).ToList();
                pathNodes.AddRange(placedNodesDict.Values.Where(x => x.PathEntity == NodeObject.PathFor.Vehicle && (x as VehicleNode).hasPathWithSidewalk == true).ToList());
            }

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