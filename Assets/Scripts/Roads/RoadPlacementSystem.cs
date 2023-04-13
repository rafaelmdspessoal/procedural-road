using UI.Controller;
using UnityEngine;
using System;
using Nodes;
using System.Collections.Generic;
using World;
using System.Linq;
using UI.Roads.Controller;
using Roads.Manager;
using Roads.Utilities;
using Rafael.Utils;
using Road.Placement.States;

namespace Roads.Placement {

    public class RoadPlacementSystem : MonoBehaviour {

        public static RoadPlacementSystem Instance { get; private set; }

        public enum NodeBuildingState {
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
        private List<RoadObject> affectedRoadsList = new();

        private RoadUIController roadUIController;
        private InputManager inputManager;
        private UIController uIController;
        private RoadManager roadManager;

        private RoadObjectSO roadObjectSO;

        private NodeBuildingState nodeBuildingState;
        private AngleSnap snappingAngle;

        private GameObject nodeGFX;

        private Vector3 controlPosition;
        private int angleToSnap;
        private int minAllowedAngle;
        private bool canBuildRoad;

        private Node startNode;
        private Node endNode;

        private IBuildingState buildingState;


        private void Awake() {
            Instance = this;
        }

        private void Start() {
            roadUIController = RoadUIController.Instance;
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;
            roadManager = RoadManager.Instance;

            nodeBuildingState = NodeBuildingState.StartNode;
            snappingAngle = AngleSnap.Zero;
            angleToSnap = 0;
            minAllowedAngle = 40;
            canBuildRoad = true;
            controlPosition = Vector3.negativeInfinity;

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
            inputManager.OnNodePlaced += InputManager_OnNodePlaced;
        }

        private void Update()
        {
            if (buildingState == null) return;

            Vector3 hitPosition;
            GameObject hitObject;

            if (RafaelUtils.TryRaycastObject(out RaycastHit hit))
            {
                hitObject = hit.transform.gameObject;
                hitPosition = RoadUtilities.GetHitPosition(hit.point, hitObject);
                hitPosition = GetPositionForMinRoadLengh(hitPosition);
                nodeGFX.transform.position = hitPosition;

                if (startNode != null)
                {
                    canBuildRoad = ValidateAngle(hitObject, hitPosition);
                    if (IsSnappingAngle && CanSnap(hitObject))
                    {
                        // if we hit ground or a road
                        hitPosition = RoadUtilities.GetHitPositionWithSnapping(hitPosition, startNode, angleToSnap);
                    }
                }

                buildingState.UpdateState(hitPosition, roadObjectSO, canBuildRoad);
            }
        }

        private bool ValidateAngle(GameObject hitObject, Vector3 hitPosition)
        {
            if (CheckRoadAngleInRange(StartPosition - ControlPosition))
                return CheckRoadAngleInRange(ControlPosition - hitPosition, hitObject, hitPosition);

            return false;
        }

        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e)
        {
            Vector3 hitPosition = RoadUtilities.GetHitPosition(e.position, e.obj, true);

            if (startNode != null && IsSnappingAngle && CanSnap(e.obj))
            {
                // if we hit ground or a road
                hitPosition = RoadUtilities.GetHitPositionWithSnapping(hitPosition, startNode, angleToSnap);
            }

            hitPosition = GetPositionForMinRoadLengh(hitPosition);
            buildingState.OnAction(hitPosition, canBuildRoad);
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
            ResetBuildingState();
        }

        private void InputManager_OnEscape()
        {
            ResetBuildingState();
        }

        private void RoadUIController_OnBuildingStraightRoad(RoadObjectSO roadObjectSO) {
            ResetBuildingState();
            this.roadObjectSO = roadObjectSO;
            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadObjectSO);
            else nodeGFX.SetActive(true);

            buildingState = new BuildingStraightRoad(this);
        }

        private void RoadUIController_OnBuildingCurvedRoad(RoadObjectSO roadObjectSO) {
            ResetBuildingState();
            this.roadObjectSO = roadObjectSO;
            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadObjectSO);
            else nodeGFX.SetActive(true);

            buildingState = new BuildingCurvedRoad(this);
        }

        private void RoadUIController_OnBuildingFreeRoad(RoadObjectSO roadObjectSO) {
            ResetBuildingState();
            this.roadObjectSO = roadObjectSO;
            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadObjectSO);
            else nodeGFX.SetActive(true);

            buildingState = new BuildingFreeRoad(this);
        }

        private void ResetBuildingState()
        {
            ClearAffectedRoads();
            ResetDisplayRoad();
            buildingState = null;
            nodeBuildingState = NodeBuildingState.StartNode;
            if (nodeGFX != null) nodeGFX.SetActive(false);
        }

        public Vector3 GetPositionForMinRoadLengh(Vector3 position) {
            if (startNode != null) {
                Vector3 roadDir = position - startNode.Position;
                float minRoadLengh = roadObjectSO.roadWidth * 1.5f;

                if (roadDir.magnitude < minRoadLengh)
                    position += roadDir.normalized * minRoadLengh - roadDir;
            }

            return position;
        }
        public bool IsBuildingStartNode() => nodeBuildingState == NodeBuildingState.StartNode;
        public bool IsBuildingControlNode() => nodeBuildingState == NodeBuildingState.ControlNode;
        public bool IsBuildingEndNode() => nodeBuildingState == NodeBuildingState.EndNode;
        
       public Node StartNode { get { return startNode; } }
        public Vector3 StartPosition { 
            get { return startNode.Position; } 
            set { startNode = roadManager.GetOrCreateNodeAt(value); } 
        }
        public Vector3 EndPosition
        {
            get { return endNode.Position; }
            set { endNode = roadManager.GetOrCreateNodeAt(value); }
        }
        public Vector3 ControlPosition
        {
            get { return controlPosition; }
            set { controlPosition = value; }
        }
        public bool IsSnappingAngle => snappingAngle != AngleSnap.Zero;

        private void ResetDisplayRoad() {
            ResetRoadPositions();
            canBuildRoad = true;
            if (buildingState != null)
                buildingState.StopPreviewDisplay();
        }
        private void ResetRoadPositions() {
            if (startNode != null && !startNode.HasConnectedRoads) {
                Destroy(startNode.gameObject);
            }
            if (endNode != null && !endNode.HasConnectedRoads) {
                Destroy(endNode.gameObject);
            }
            controlPosition = Vector3.negativeInfinity;
            startNode = null;
            endNode = null;
        }
        private void InputManager_OnCancel() {
            nodeBuildingState = NodeBuildingState.StartNode;
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
            GameObject controlNodeObject = RoadUtilities.CreateControlNode(roadObjectSO, controlNodePosition);

            roadObject.PlaceRoad(startNode, endNode, controlNodeObject);
            affectedRoadsList.Add(roadObject);
            affectedRoadsList.AddRange(roadObject.GetAllConnectedRoads());
            return roadObject;
        }

        public void SplitRoads() {
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

        public void SetRoadsMesh()
        {
            foreach (RoadObject roadObject in affectedRoadsList)
            {
                roadObject.SetMesh();
            }
            affectedRoadsList.Clear();
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

        public void UpdateBuildingState(NodeBuildingState state)
        {
            nodeBuildingState = state;
        }
    }
}