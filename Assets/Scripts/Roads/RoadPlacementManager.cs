using UI.Controller;
using UnityEngine;
using System;
using MeshHandler.Road.Temp.Builder;
using UI.Controller.Road;
using Road.Manager;

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

        public EventHandler<OnNodesPlacedEventArgs> OnNodesPlaced;

        public class OnNodesPlacedEventArgs : EventArgs {
            public Vector3 startNodePosition;
            public Vector3 controlNodePosition;
            public Vector3 endNodePosition;
            public RoadObjectSO roadObjectSO;
        }

        [SerializeField] private Material temporaryRoadMaterial;
        [SerializeField] private GameObject nodeGFX;

        private RoadUIController roadUIController;
        private InputManager inputManager;
        private UIController uIController;
        private RoadManager roadManager;

        private RoadObjectSO roadObjectSO;

        private State state;
        private BuildingState buildingState;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

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

            roadUIController.OnBuildingRoad += RoadUIController_OnBuildingRoad;
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
            roadManager.ClearAffectedRoads();
            ResetDisplayRoad();
            if (nodeGFX != null) nodeGFX.SetActive(false);
            state = State.Idle;
            Debug.Log("Road Placement State: " + state);
        }

        private void InputManager_OnEscape() {
            Debug.Log("Escaped");
            roadManager.ClearAffectedRoads();
            ResetDisplayRoad();
            if (nodeGFX != null) nodeGFX.SetActive(false);
            state = State.Idle;
        }

        private void RoadUIController_OnBuildingRoad(RoadObjectSO roadObjectSO) {
            roadManager.ClearAffectedRoads();
            ResetDisplayRoad();
            state = State.StraightRoad;
            Debug.Log("Building Road: " + state);
            this.roadObjectSO = roadObjectSO;
        }

        public bool IsBuilding() {
            return state != State.Idle && state != State.RemovingRoad;
        }

        public BuildingState GetBuildingState() => buildingState;

        public void UpdateBuildingState(BuildingState state) {
            buildingState = state;
        }

        public bool IsBuildingStraightRoad() => state == State.StraightRoad;

        public RoadObjectSO GetRoadObjectSO() => roadObjectSO;

        private void ResetDisplayRoad() {
            UpdateBuildingState(BuildingState.StartNode);
            meshFilter.mesh = null;
        }
        private void InputManager_OnCancel() {
            Debug.Log("Building cancelled");
            roadManager.ClearAffectedRoads();
            ResetDisplayRoad();
        }
    }
}