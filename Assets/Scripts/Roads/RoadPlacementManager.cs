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
using World;
using System.Linq;

namespace Road.Placement {

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RoadPlacementManager : MonoBehaviour {

        public static RoadPlacementManager Instance { get; private set; }

        private enum State {
            Idle,
            StraightRoad,
            CurvedRoad,
            FreeRoad,
            RemovingRoad,
        }

        public enum BuildingState {
            StartNode,
            ControlNode,
            EndNode,
        }

        private enum AngleSnap {
            Zero,
            Five,
            Ten,
            Fifteen,
        }

        public Action<int> OnAngleSnapChanged;
        public EventHandler<OnRoadPlacedEventArgs> OnRoadPlaced;
        public class OnRoadPlacedEventArgs : EventArgs {
            public RoadObject roadObject;
        }

        private readonly Dictionary<Vector3, RoadObject> roadsToSplit = new();

        [SerializeField] private Material temporaryRoadMaterial;
        [SerializeField] private Material cantBuildRoadMaterial;

        private RoadUIController roadUIController;
        private InputManager inputManager;
        private UIController uIController;
        private RoadManager roadManager;

        private RoadObjectSO roadObjectSO;

        private State state;
        private BuildingState buildingState;
        private AngleSnap snappingAngle;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private GameObject nodeGFX;

        private Vector3 controlPosition;
        private int angleToSnap;
        private int minAllowedAngle;
        private bool canBuildRoad;

        private Node startNode;
        private Node endNode;


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

            state = State.Idle;
            buildingState = BuildingState.StartNode;
            snappingAngle = AngleSnap.Zero;
            angleToSnap = 0;
            minAllowedAngle = 30;
            canBuildRoad = true;

            roadUIController.OnBuildingStraightRoad += RoadUIController_OnBuildingStraightRoad;
            roadUIController.OnBuildingCurvedRoad += RoadUIController_OnBuildingCurvedRoad;
            roadUIController.OnBuildingFreeRoad += RoadUIController_OnBuildingFreeRoad;

            roadUIController.OnGridSnapping += RoadUIController_OnGridSnapping;
            roadUIController.OnAngleSnapping += RoadUIController_OnAngleSnapping;
            roadUIController.OnRoadUp += RoadUIController_OnRoadUp;
            roadUIController.OnRoadDown += RoadUIController_OnRoadDown;

            uIController.OnRemovingObjects += UIController_OnRemovingObjects;

            inputManager.OnEscape += InputManager_OnEscape;
            inputManager.OnCancel += InputManager_OnCancel;
        }

        private void RoadUIController_OnRoadDown() {
            throw new NotImplementedException();
        }

        private void RoadUIController_OnRoadUp() {
            throw new NotImplementedException();
        }

        private void RoadUIController_OnAngleSnapping() {
            switch (snappingAngle) {
                default:
                    snappingAngle = AngleSnap.Zero;
                    angleToSnap = 0;
                    break;
                case AngleSnap.Zero:
                    snappingAngle = AngleSnap.Five;
                    angleToSnap = 5;
                    break;
                case AngleSnap.Five:
                    snappingAngle = AngleSnap.Ten;
                    angleToSnap = 10;
                    break;
                case AngleSnap.Ten:
                    snappingAngle = AngleSnap.Fifteen;
                    angleToSnap = 15;
                    break;
                case AngleSnap.Fifteen:
                    snappingAngle = AngleSnap.Zero;
                    angleToSnap = 0;
                    break;
            }
            OnAngleSnapChanged?.Invoke(angleToSnap);
        }

        private void RoadUIController_OnGridSnapping() {
            throw new NotImplementedException();
        }

        public void DisplayTemporaryMesh(Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition) {
            RoadTempMeshBuilder tempMeshBuilder = new(
                startPosition,
                endPosition,
                controlPosition,
                roadObjectSO.roadResolution,
                roadObjectSO.roadWidth);

            Mesh mesh = tempMeshBuilder.CreateTempRoadMesh();

            if (canBuildRoad == false)
                meshRenderer.sharedMaterial = cantBuildRoadMaterial;
            else 
                meshRenderer.sharedMaterial = temporaryRoadMaterial;           

            meshFilter.mesh = mesh;
        }

        /// <summary>
        /// Handles first node placement
        /// </summary>
        /// <param name="roadDirection">The direction of reference to check the angle</param>
        /// <returns></returns>
        public bool CheckRoadAngleInRange(Vector3 roadDirection) {
            canBuildRoad = true;
            if (startNode == null && roadsToSplit.Count <= 0) {
                return canBuildRoad;
            }

            if (roadsToSplit.Count > 0) {
                RoadObject roadObj = roadsToSplit.First().Value;
                Vector3 hitPosition = roadsToSplit.First().Key;

                Vector3 nextRoadDirection = Bezier.GetTangentAt(roadObj, hitPosition, out _, out _);
                float angle = Vector3.Angle(nextRoadDirection, roadDirection);
                if (angle < minAllowedAngle || angle > 180 - minAllowedAngle) {
                    print("hitRoadAt: " + angle);
                    canBuildRoad = false;
                    return canBuildRoad;
                }
            }

            foreach (RoadObject roadObject in startNode.ConnectedRoads) {
                Vector3 prevRoadDirection = startNode.Position - roadObject.ControlPosition;
                float angle = Vector3.Angle(prevRoadDirection, roadDirection);
                if (angle < minAllowedAngle)
                    return canBuildRoad = false;
            }

            return canBuildRoad;
        }

        /// <summary>
        /// Handle cases for secon node placement
        /// </summary>
        /// <param name="roadDirection">The direction of the secment being placed</param>
        /// <param name="hitObj">The object we hit (grond, node or road)</param>
        /// <returns></returns>
        public bool CheckRoadAngleInRange(Vector3 roadDirection, GameObject hitObj, Vector3 hitPosition) {
            canBuildRoad = true;

            if (hitObj.TryGetComponent(out Node node)) {
                foreach (RoadObject roadObject in node.ConnectedRoads) {
                    // TODO If the ROad is curved the direction will depend on which half of
                    // the road we hit. This must be accounteable.
                    Vector3 nextRoadDirection = roadObject.ControlPosition - node.Position;
                    float angle = Vector3.Angle(nextRoadDirection, roadDirection);
                    if (angle < minAllowedAngle) {
                        print("hitNodeAt: " + angle);
                        canBuildRoad = false;
                        return canBuildRoad;
                    }
                }
            }                

            if (hitObj.TryGetComponent(out RoadObject roadObj)) {
                // We are endind our road on top of another
                // In case of the road is curved we must get the tangent at hitPosition to
                // calculate the correct angle.
                Vector3 nextRoadDirection = Bezier.GetTangentAt(roadObj, hitPosition, out _, out _);
                float angle = Vector3.Angle(nextRoadDirection, roadDirection);
                if (angle < minAllowedAngle || angle > 180 - minAllowedAngle) {
                    print("hitRoadAt: " + angle);
                    canBuildRoad = false;
                    return canBuildRoad;
                }
            }

            return canBuildRoad;
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

        private void RoadUIController_OnBuildingFreeRoad(RoadObjectSO roadObjectSO) {
            ClearAffectedRoads();
            ResetDisplayRoad();
            state = State.FreeRoad;
            Debug.Log("Building Road: " + state);
            this.roadObjectSO = roadObjectSO;
            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadObjectSO);
            else nodeGFX.SetActive(true);
        }
        public Vector3 GetPositionForMinRoadLengh(Vector3 position) {
            if (startNode != null) {
                Vector3 roadDir = position - startNode.Position;
                float minRoadLengh = 2 * roadObjectSO.GetMaxNodeSize();

                if (roadDir.magnitude < minRoadLengh)
                    position += roadDir.normalized * minRoadLengh - roadDir;
            }

            return position;
        }
        public bool IsBuilding() => state != State.Idle && state != State.RemovingRoad;
        public void UpdateBuildingState(BuildingState state) => buildingState = state;
        public bool IsBuildingStraightRoad => state == State.StraightRoad;
        public bool IsBuildingCurvedRoad => state == State.CurvedRoad;
        public bool IsBuildingFreeRoad => state == State.FreeRoad;
        public bool IsBuildingStartNode() => buildingState == BuildingState.StartNode;
        public bool IsBuildingControlNode() => buildingState == BuildingState.ControlNode;
        public bool IsBuildingEndNode() => buildingState == BuildingState.EndNode;
        public void SetNodeGFXPosition(Vector3 position) => nodeGFX.transform.position = position;
        public Node StartNode => startNode;
        public Vector3 StartPosition { 
            get { return startNode.Position; } 
            set { startNode = roadManager.GetOrCreateNodeAt(value); } 
        }
        public bool CanBuildStraightRoad {
            get { return (canBuildRoad && IsBuilding() && IsBuildingStraightRoad); }
            set { canBuildRoad = value; }
        }
        public bool CanBuildCurvedRoad {
            get { return (canBuildRoad && IsBuilding() && IsBuildingCurvedRoad); }
            set { canBuildRoad = value; }
        }
        public bool CanBuildFreeRoad {
            get { return (canBuildRoad && IsBuilding() && IsBuildingFreeRoad); }
            set { canBuildRoad = value; }
        }
        public Vector3 ControlPosition { get { return controlPosition; } set { controlPosition = value; } }
        public Vector3 EndPosition { set { endNode = roadManager.GetOrCreateNodeAt(value); } }
        public bool IsSnappingAngle => snappingAngle != AngleSnap.Zero;
        public int AngleToSnap => angleToSnap;

        private void ResetDisplayRoad() {
            UpdateBuildingState(BuildingState.StartNode);
            ResetRoadPositions();
            startNode = null;
            canBuildRoad = true;
            meshFilter.mesh = null;
        }
        private void ResetRoadPositions() {
            controlPosition = Vector3.negativeInfinity;

            if (startNode != null && !startNode.HasConnectedRoads) {
                Destroy(startNode.gameObject);
                startNode = null;
            }
            if (endNode != null && !endNode.HasConnectedRoads) {
                Destroy(endNode.gameObject);
                endNode = null;
            }
        }
        private void InputManager_OnCancel() {
            Debug.Log("Building cancelled");
            ClearAffectedRoads();
            ResetDisplayRoad();
        }

        public void PlaceRoad() {
            CreateRoadObject(startNode, endNode, controlPosition, roadObjectSO);
            Node cachedEndNode = endNode;
            ResetRoadPositions();
            startNode = cachedEndNode;
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

        public bool CanSnap(GameObject hitObject) {
            if (hitObject.TryGetComponent(out Ground _) || hitObject.TryGetComponent(out RoadObject _))
                return true;
            return false;
        }
    }
}