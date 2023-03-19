using System;
using System.Collections.Generic;
using UnityEngine;
using Road.Placement;
using Road.Obj;
using Road.NodeObj;
using Road.Utilities;

namespace Road.Manager {

    public class RoadManager : MonoBehaviour {
        private RoadPlacementManager roadPlacementManager;

        public static RoadManager Instance { get; private set; }

        private readonly Dictionary<Vector3, Node> placedNodesDict = new();

        public EventHandler<OnRoadCreatedEventArgs> OnRoadCreated;
        public class OnRoadCreatedEventArgs : EventArgs { public RoadObject roadObject; }

        public EventHandler<OnRoadsUpdatedEventArgs> OnRoadsUpdated;
        public class OnRoadsUpdatedEventArgs : EventArgs { public List<RoadObject> roadsToUpdate; }


        [SerializeField] private Transform roadParentTransform;
        [SerializeField] private Transform nodeParentTransform;
        [SerializeField] private GameObject node;
        
        public bool updateRoads = false;

        private readonly Dictionary<Vector3, RoadObject> roadsToSplit = new();

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            roadPlacementManager = RoadPlacementManager.Instance;
            roadPlacementManager.OnNodesPlaced += RoadPlacementManager_OnFinishPlacingNodes;
        }

        private void RoadPlacementManager_OnFinishPlacingNodes(object sender, RoadPlacementManager.OnNodesPlacedEventArgs e) {
            Node startNode = GetOrCreateNodeAt(e.startNodePosition);
            Node endNode = GetOrCreateNodeAt(e.endNodePosition);
            RoadObjectSO roadObjectSO = e.roadObjectSO;

            RoadObject newRoadObject = CreateRoadObject(startNode, endNode, e.controlNodePosition, roadObjectSO);

            SplitRoads();
            OnRoadCreated?.Invoke(this, new OnRoadCreatedEventArgs { roadObject = newRoadObject });
        }

        private void RoadObject_OnRoadBuilt(object sender, RoadObject.OnRoadChangedEventArgs e) {
            RoadObject newRoad = e.roadObject;
            newRoad.SetRoadMesh();
            List<RoadObject> connectedRoads = newRoad.GetAllConnectedRoads();
            foreach (RoadObject roadObj in connectedRoads) {
                roadObj.UpdateRoadMesh();
            }
            Debug.Log("All Connected Roads: " + connectedRoads.Count);
            newRoad.OnRoadBuilt -= RoadObject_OnRoadBuilt;
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

        public Node GetNodeAt(Vector3 position) {
            return placedNodesDict.GetValueOrDefault(position);
        }

        public void AddNode(Node node) {
            if (!HasNode(node))
                placedNodesDict.Add(node.Position, node);
        }

        private bool HasNode(Node node) {
            return placedNodesDict.ContainsValue(node);
        }

        private bool HasNode(Vector3 position) {
            return placedNodesDict.ContainsKey(position);
        }

        private Node GetOrCreateNodeAt(Vector3 position) {
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



        private RoadObject CreateRoadObject(Node startNode, Node endNode, Vector3 controlNodePosition, RoadObjectSO roadObjectSO) {
            Vector3 roadPosition = (startNode.gameObject.transform.position + endNode.gameObject.transform.position) / 2;
            GameObject roadGameObject = Instantiate(roadObjectSO.roadObjectPrefab, roadPosition, Quaternion.identity, roadParentTransform);
            RoadObject roadObject = roadGameObject.GetComponent<RoadObject>();

            roadObject.OnRoadRemoved += RoadObject_OnRoadRemoved;
            roadObject.OnRoadUpdated += RoadObject_OnRoadUpdated;
            roadObject.OnRoadBuilt += RoadObject_OnRoadBuilt;

            GameObject controlNodeObject = RoadUtilities.CreateControlNode(roadObject.GetRoadObjectSO, controlNodePosition);
            roadObject.BuildRoad(startNode, endNode, controlNodeObject);

            return roadObject;
        }

        public void AddRoadToSplit(Vector3 position, RoadObject roadObject) {
            if (!roadsToSplit.ContainsKey(position)) {
                roadsToSplit.Add(position, roadObject);
            }
        }

        public void ClearAffectedRoads() {
            roadsToSplit.Clear();
        }

        private void SplitRoads() {
            Debug.Log("Roads to split: " + roadsToSplit.Count);
            foreach (Vector3 positionToSplit in roadsToSplit.Keys) {
                RoadObject roadToSplit = roadsToSplit[positionToSplit];
                Node startNode = roadToSplit.StartNode;
                Node centerNode = GetNodeAt(positionToSplit);
                Node endNode = roadToSplit.EndNode;

                Bezier.GetTangentAt(
                    roadToSplit,
                    centerNode.Position,
                    out Vector3 newStartControlPointPosition,
                    out Vector3 newEndControlPointPosition);

                roadToSplit.Remove(true);
                CreateRoadObject(startNode, centerNode, newStartControlPointPosition, roadToSplit.GetRoadObjectSO);
                CreateRoadObject(centerNode, endNode, newEndControlPointPosition, roadToSplit.GetRoadObjectSO);
            }
            ClearAffectedRoads();
        }
    }
}