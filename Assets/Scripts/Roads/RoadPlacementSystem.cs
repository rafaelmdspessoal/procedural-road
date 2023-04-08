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
using Roads.Preview.MeshHandler;
using Roads.Preview;
using Rafael.Utils;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

namespace Roads.Placement {

    public class RoadPlacementSystem : MonoBehaviour {

        public static RoadPlacementSystem Instance { get; private set; }

        public enum NodeBuilding {
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

        private RoadUIController roadUIController;
        private InputManager inputManager;
        private UIController uIController;
        private RoadManager roadManager;
        private RoadPreviewSystem roadPreviewSystem;

        private RoadObjectSO roadObjectSO;

        private NodeBuilding nodeBuildingState;
        private AngleSnap snappingAngle;

        private GameObject nodeGFX;

        private Vector3 controlPosition;
        private int angleToSnap;
        private int minAllowedAngle;
        private bool canBuildRoad;

        private Node startNode;
        private Node endNode;


        private void Awake() {
            Instance = this;
        }

        private void Start() {
            roadUIController = RoadUIController.Instance;
            roadPreviewSystem = RoadPreviewSystem.Instance;
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;
            roadManager = RoadManager.Instance;

            nodeBuildingState = NodeBuilding.StartNode;
            snappingAngle = AngleSnap.Zero;
            angleToSnap = 0;
            minAllowedAngle = 30;
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
            Vector3 hitPosition;
            GameObject hitObj;
            if (RafaelUtils.TryRaycastObject(out RaycastHit hit))
            {
                hitObj = hit.transform.gameObject;
                hitPosition = RoadUtilities.GetHitPosition(hit.point, hitObj);
                nodeGFX.transform.position = hitPosition;
            }
            roadPreviewSystem.DisplayTemporaryMesh(StartPosition, ControlPosition, EndPosition, roadObjectSO.roadWidth, roadObjectSO.roadResolution, true);
        }
        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e)
        {
            Vector3 hitPosition = RoadUtilities.GetHitPosition(e.position, e.obj, true);
            Debug.Log("Node Placed!");
            if (IsBuildingStartNode())
            {
                StartPosition = hitPosition;
                nodeBuildingState = NodeBuilding.ControlNode;
                return;
            }


            if (IsBuildingControlNode())
            {
                if (controlPosition != Vector3.negativeInfinity)
                    nodeBuildingState = NodeBuilding.EndNode;

                if (IsSnappingAngle && e.obj.TryGetComponent(out Ground _))
                {
                    // Only tries to snap if we hit ground
                    hitPosition = RoadUtilities.GetHitPositionWithSnapping(hitPosition, startNode, 15);
                }

                ControlPosition = GetPositionForMinRoadLengh(hitPosition);
                nodeBuildingState = NodeBuilding.EndNode;
                return;
            }


            if (IsBuildingEndNode())
            {
                hitPosition = RoadUtilities.GetHitPosition(hitPosition, e.obj, true);
                GetPositionForMinRoadLengh(hitPosition);
                EndPosition = hitPosition;
                PlaceRoad();
                SplitRoads();
                return;
            }
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
        }

        private void InputManager_OnEscape() {
            Debug.Log("Escaped");
            ClearAffectedRoads();
            ResetDisplayRoad();
            if (nodeGFX != null) nodeGFX.SetActive(false);
        }

        private void RoadUIController_OnBuildingStraightRoad(RoadObjectSO roadObjectSO) {
            ClearAffectedRoads();
            ResetDisplayRoad();
            this.roadObjectSO = roadObjectSO;
            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadObjectSO);
            else nodeGFX.SetActive(true);
        }

        private void RoadUIController_OnBuildingCurvedRoad(RoadObjectSO roadObjectSO) {
            ClearAffectedRoads();
            ResetDisplayRoad();
            this.roadObjectSO = roadObjectSO;
            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadObjectSO);
            else nodeGFX.SetActive(true);
        }

        private void RoadUIController_OnBuildingFreeRoad(RoadObjectSO roadObjectSO) {
            ClearAffectedRoads();
            ResetDisplayRoad();
            this.roadObjectSO = roadObjectSO;
            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadObjectSO);
            else nodeGFX.SetActive(true);
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
        public bool IsBuildingStartNode() => nodeBuildingState == NodeBuilding.StartNode;
        public bool IsBuildingControlNode() => nodeBuildingState == NodeBuilding.ControlNode;
        public bool IsBuildingEndNode() => nodeBuildingState == NodeBuilding.EndNode;
        
       
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
        public int AngleToSnap => angleToSnap;

        private void ResetDisplayRoad() {
            ResetRoadPositions();
            startNode = null;
            canBuildRoad = true;
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
            GameObject controlNodeObject = RoadUtilities.CreateControlNode(roadObjectSO, controlNodePosition);

            roadObject.PlaceRoad(startNode, endNode, controlNodeObject);
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