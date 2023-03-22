using UI.Controller;
using UnityEngine;
using System;
using MeshHandler.Road.Temp.Builder;
using UI.Controller.Road;
using Road.Manager;
using Road.Utilities;
using Road.Obj;
using Road.NodeObj;
using System.Collections.Generic;

namespace Road.Placement {

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RoadPlacementManager : MonoBehaviour {

        public static RoadPlacementManager Instance { get; private set; }

        private enum State {
            Idle,
            StraightRoad,
            CurvedRoad,
            RemovingRoad,
        }

        public enum BuildingState {
            StartNode,
            ControlNode,
            EndNode,
        }

        public EventHandler<OnRoadPlacedEventArgs> OnRoadPlaced;

        public class OnRoadPlacedEventArgs : EventArgs {
            public RoadObject roadObject;
        }


        private readonly Dictionary<Vector3, RoadObject> roadsToSplit = new();

        [SerializeField] private Material temporaryRoadMaterial;
        [SerializeField] private float angleSnap;

        private RoadUIController roadUIController;
        private InputManager inputManager;
        private UIController uIController;
        private RoadManager roadManager;

        private RoadObjectSO roadObjectSO;

        private State state;
        private BuildingState buildingState;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private GameObject nodeGFX;


        private void Awake() {
            Instance = this;
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Start() {
            roadUIController = RoadUIController.Instance;
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;
            roadManager = RoadManager.Instance;

            roadUIController.OnBuildingStraightRoad += RoadUIController_OnBuildingStraightRoad;
            roadUIController.OnBuildingCurvedRoad += RoadUIController_OnBuildingCurvedRoad;
            inputManager.OnEscape += InputManager_OnEscape;
            inputManager.OnCancel += InputManager_OnCancel;
            uIController.OnRemovingObjects += UIController_OnRemovingObjects;

            state = State.Idle;
            buildingState = BuildingState.StartNode;
        }

        public void DisplayTemporaryMesh(Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition) {
            RoadTempMeshBuilder tempMeshBuilder = new(
                startPosition,
                endPosition,
                controlPosition,
                roadObjectSO.roadResolution,
                roadObjectSO.roadWidth);

            Mesh mesh = tempMeshBuilder.CreateTempRoadMesh();
            meshRenderer.sharedMaterial = temporaryRoadMaterial;
            meshFilter.mesh = mesh;
        }

        private void UIController_OnRemovingObjects() {
            ClearAffectedRoads();
            ResetDisplayRoad();
            if (nodeGFX != null) nodeGFX.SetActive(false);
            state = State.Idle;
            Debug.Log("Road Placement State: " + state);
        }

        private void InputManager_OnEscape() {
            Debug.Log("Escaped");
            ClearAffectedRoads();
            ResetDisplayRoad();
            if (nodeGFX != null) nodeGFX.SetActive(false);
            state = State.Idle;
        }

        private void RoadUIController_OnBuildingStraightRoad(RoadObjectSO roadObjectSO) {
            ClearAffectedRoads();
            ResetDisplayRoad();
            state = State.StraightRoad;
            Debug.Log("Building Road: " + state);
            this.roadObjectSO = roadObjectSO;
            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadObjectSO);
            else nodeGFX.SetActive(true);
        }

        private void RoadUIController_OnBuildingCurvedRoad(RoadObjectSO roadObjectSO) {
            ClearAffectedRoads();
            ResetDisplayRoad();
            state = State.CurvedRoad;
            Debug.Log("Building Road: " + state);
            this.roadObjectSO = roadObjectSO;
            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadObjectSO);
            else nodeGFX.SetActive(true);
        }

        public bool IsBuilding() => state != State.Idle && state != State.RemovingRoad;
        public BuildingState GetBuildingState() => buildingState;
        public void UpdateBuildingState(BuildingState state) => buildingState = state;
        public bool IsBuildingStraightRoad() => state == State.StraightRoad;
        public bool IsBuildingCurvedRoad() => state == State.CurvedRoad;
        public bool IsBuildingStartNode() => buildingState == BuildingState.StartNode;
        public bool IsBuildingControlNode() => buildingState == BuildingState.ControlNode;
        public bool IsBuildingEndNode() => buildingState == BuildingState.EndNode;
        public RoadObjectSO GetRoadObjectSO() => roadObjectSO;

        private void ResetDisplayRoad() {
            UpdateBuildingState(BuildingState.StartNode);
            meshFilter.mesh = null;
        }
        private void InputManager_OnCancel() {
            Debug.Log("Building cancelled");
            ClearAffectedRoads();
            ResetDisplayRoad();
        }

        public void SetNodeGFXPosition(Vector3 position) {
            nodeGFX.transform.position = position;
        }

        public void PlaceRoad( Vector3 startPosition, Vector3 controlPosition, Vector3 endPosition) {
            Node startNode = roadManager.GetOrCreateNodeAt(startPosition);
            Node endNode = roadManager.GetOrCreateNodeAt(endPosition);

            CreateRoadObject(startNode, endNode, controlPosition, roadObjectSO);
        }

        private RoadObject CreateRoadObject(Node startNode, Node endNode, Vector3 controlNodePosition, RoadObjectSO roadObjectSO) {
            Vector3 roadPosition = (startNode.gameObject.transform.position + endNode.gameObject.transform.position) / 2;
            GameObject roadGameObject = Instantiate(
                roadObjectSO.roadObjectPrefab, 
                roadPosition, 
                Quaternion.identity, 
                roadManager.GetRoadParent());
            RoadObject roadObject = roadGameObject.GetComponent<RoadObject>();
            GameObject controlNodeObject = RoadUtilities.CreateControlNode(roadObject.GetRoadObjectSO, controlNodePosition);

            roadObject.PlaceRoad(startNode, endNode, controlNodeObject);
            OnRoadPlaced?.Invoke(this, new OnRoadPlacedEventArgs { roadObject = roadObject });
            return roadObject;
        }

        public void SplitRoads() {
            Debug.Log("Roads to split: " + roadsToSplit.Count);
            foreach (Vector3 positionToSplit in roadsToSplit.Keys) {
                RoadObject roadToSplit = roadsToSplit[positionToSplit];
                Node startNode = roadToSplit.StartNode;
                Node centerNode = roadManager.GetNodeAt(positionToSplit);
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

        private void ClearAffectedRoads() {
            roadsToSplit.Clear();
        }

        public void AddRoadToSplit(Vector3 position, RoadObject roadObject) {
            if (!roadsToSplit.ContainsKey(position)) {
                roadsToSplit.Add(position, roadObject);
            }
        }
    }
}