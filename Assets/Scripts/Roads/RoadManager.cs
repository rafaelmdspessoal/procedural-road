using System;
using System.Collections.Generic;
using UnityEngine;
using Road.Placement;
using Road.Obj;
using Road.NodeObj;
using Road.Utilities;

namespace Road.Manager {

    public class RoadManager : MonoBehaviour {
        public static RoadManager Instance { get; private set; }

        private RoadPlacementManager roadPlacementManager;
        public bool updateRoads = false;
        private readonly Dictionary<Vector3, Node> placedNodesDict = new();

        [SerializeField] private Transform roadParentTransform;
        [SerializeField] private Transform nodeParentTransform;
        [SerializeField] private GameObject node;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            roadPlacementManager = RoadPlacementManager.Instance;
            roadPlacementManager.OnRoadPlaced += RoadPlacementManager_OnRoadPlaced;
        }

        private void RoadPlacementManager_OnRoadPlaced(object sender, RoadPlacementManager.OnRoadPlacedEventArgs e) {
            RoadObject roadObject = e.roadObject;
            roadObject.OnRoadRemoved += RoadObject_OnRoadRemoved;
            roadObject.OnRoadUpdated += RoadObject_OnRoadUpdated;
            roadObject.OnRoadBuilt += RoadObject_OnRoadBuilt;
            roadObject.OnRoadPlaced += RoadObject_OnRoadPlaced;
        }

        private void RoadObject_OnRoadPlaced(object sender, RoadObject.OnRoadChangedEventArgs e) {
            throw new NotImplementedException();
        }

        private void RoadObject_OnRoadBuilt(object sender, RoadObject.OnRoadChangedEventArgs e) {
            throw new NotImplementedException();
        }

        private void RoadObject_OnRoadUpdated(object sender, RoadObject.OnRoadChangedEventArgs e) {
            throw new NotImplementedException();
        }

        private void RoadObject_OnRoadRemoved(object sender, RoadObject.OnRoadChangedEventArgs e) {
            List<RoadObject> connectedRoads = e.roadObject.GetAllConnectedRoads();
            foreach (RoadObject roadObj in connectedRoads) {
                roadObj.UpdateRoadMesh();
            }
        }

        public void AddNode(Node node) {
            if (!HasNode(node))
                placedNodesDict.Add(node.Position, node);
        }

        public Node GetNodeAt(Vector3 position) => placedNodesDict.GetValueOrDefault(position);
        private bool HasNode(Node node) => placedNodesDict.ContainsValue(node);
        private bool HasNode(Vector3 position) => placedNodesDict.ContainsKey(position);
        public Transform GetRoadParent() => roadParentTransform;
        public Node GetOrCreateNodeAt(Vector3 position) {
            if (HasNode(position)) {
                Node existingNode = GetNodeAt(position);
                Debug.Log("existingNode: " + existingNode);
                return existingNode;
            }

            GameObject nodeObject = Instantiate(node, position, Quaternion.identity, nodeParentTransform);
            Node newNode = nodeObject.GetComponent<Node>();
            AddNode(newNode);
            return newNode;
        }
    }
}