using System;
using System.Collections.Generic;
using UnityEngine;
using Nodes;
using Roads.Placement;
using System.Linq;

namespace Roads.Manager {

    public class RoadManager : MonoBehaviour {
        public static RoadManager Instance { get; private set; }

        private RoadPlacementSystem roadPlacementManager;
        public bool updateRoads = false;
        private readonly Dictionary<Vector3, Node> placedNodesDict = new();

        [SerializeField] private Transform roadParentTransform;
        [SerializeField] private Transform nodeParentTransform;
        [SerializeField] private GameObject node;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            roadPlacementManager = RoadPlacementSystem.Instance;
            roadPlacementManager.OnRoadPlaced += RoadPlacementManager_OnRoadPlaced;
        }

        private void RoadPlacementManager_OnRoadPlaced(object sender, RoadPlacementSystem.OnRoadPlacedEventArgs e) {
            RoadObject roadObject = e.roadObject;
            roadObject.OnRoadRemoved += RoadObject_OnRoadRemoved;
            roadObject.OnRoadUpdated += RoadObject_OnRoadUpdated;
            roadObject.OnRoadBuilt += RoadObject_OnRoadBuilt;
            roadObject.OnRoadPlaced += RoadObject_OnRoadPlaced;
        }

        private void RoadObject_OnRoadPlaced(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void RoadObject_OnRoadBuilt(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void RoadObject_OnRoadUpdated(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void RoadObject_OnRoadRemoved(object sender, EventArgs e) {
            List<RoadObject> connectedRoads = (sender as RoadObject).GetAllConnectedRoads();
            foreach (RoadObject roadObj in connectedRoads) {
                roadObj.UpdateMesh();
            }
        }

        public void AddNode(Node node) {
            if (!HasNode(node))
                placedNodesDict.Add(node.Position, node);
        }
        public void RemoveNode(Node node)
        {
            if (HasNode(node))
            {
                placedNodesDict.Remove(node.Position);
                Destroy(node.gameObject);
            }
        }

        public Node GetNodeAt(Vector3 position) => placedNodesDict.GetValueOrDefault(position);
        private bool HasNode(Node node) => placedNodesDict.ContainsValue(node);
        public bool HasNode(Vector3 position) => placedNodesDict.ContainsKey(position);
        public Transform GetRoadParent() => roadParentTransform;
        public Node GetOrCreateNodeAt(Vector3 position) {
            if (HasNode(position)) {
                Node existingNode = GetNodeAt(position);
                return existingNode;
            }

            GameObject nodeObject = Instantiate(node, position, Quaternion.identity, nodeParentTransform);
            Node newNode = nodeObject.GetComponent<Node>();
            AddNode(newNode);
            return newNode;
        }
        public Node GetRandomNode()
        {
            return placedNodesDict.Values.ToList()[UnityEngine.Random.Range(0, placedNodesDict.Count)];
        }
        public RoadObject GetRoadBetween(Node startNode, Node endNode)
        {
            foreach (RoadObject roadObject in startNode.ConnectedRoads)
            {
                if (roadObject.OtherNodeTo(startNode).Equals(endNode))
                    return roadObject;
            }
            return null;
        }
    }
}