using UnityEngine;
using System;
using System.Collections.Generic;
using World;
using System.Linq;
using Path;
using Path.Utilities;
using Path.Placement.States;
using Path.Entities;
using Path.Entities.SO;
using Path.UI;
using Global.UI;

namespace Path.PlacementSystem {

    public class PathPlacementSystem : MonoBehaviour {

        public static PathPlacementSystem Instance { get; private set; }

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

        public EventHandler<OnPathPlacedEventArgs> OnPathPlaced;
        public class OnPathPlacedEventArgs : EventArgs
        {
            public PathObject pathObject;
        }

        private readonly Dictionary<Vector3, PathObject> pathsToSplit = new();

        private PathUIController pathUIController;
        private InputManager inputManager;
        private UIController uIController;
        private PathManager pathManager;

        private PathSO pathSO;

        private NodeBuildingState nodeBuildingState;
        private AngleSnap snappingAngle;

        private GameObject nodeGFX;

        private Vector3 controlPosition;
        private int angleToSnap;
        private int minAllowedAngle;
        private bool canBuildPath;
        private float minPathLengh;

        private NodeObject startNode;
        private NodeObject endNode;

        private IBuildingState buildingState;


        private void Awake() {
            Instance = this;
        }

        private void Start() {
            pathUIController = PathUIController.Instance;
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;
            pathManager = PathManager.Instance;

            nodeBuildingState = NodeBuildingState.StartNode;
            snappingAngle = AngleSnap.Zero;
            angleToSnap = 0;
            minAllowedAngle = 40;
            canBuildPath = true;
            controlPosition = Vector3.negativeInfinity;
            minPathLengh = Mathf.Infinity;

            pathUIController.OnBuildingStraightPath += PathUIController_OnBuildingStraightPath;
            pathUIController.OnBuildingCurvedPath += PathUIController_OnBuildingCurvedPath;
            pathUIController.OnBuildingFreePath += PathUIController_OnBuildingFreePath;

            pathUIController.OnGridSnapping += PathUIController_OnGridSnapping;
            pathUIController.OnAngleSnapping += PathUIController_OnAngleSnapping;
            pathUIController.OnPathUp += PathUIController_OnPathUp;
            pathUIController.OnPathDown += PathUIController_OnPathDown;

            uIController.OnRemovingObjects += UIController_OnRemovingObjects;

            inputManager.OnEscape += InputManager_OnEscape;
            inputManager.OnCancel += InputManager_OnCancel;
            inputManager.OnNodePlaced += InputManager_OnNodePlaced;
        }

        private void Update()
        {
            if (buildingState == null) return;

            if (PathUtilities.TryRaycastObject(out Vector3 hitPosition, out GameObject hitObject))
            {
                hitPosition = GetPositionForMinPathLengh(hitPosition);
                if (startNode != null)
                {
                    canBuildPath = ValidateAngle(hitObject, hitPosition);
                    Vector3 controlPos = (startNode.Position + hitPosition) / 2;
                    if (Bezier.GetLengh(startNode.Position, hitPosition, controlPos) < minPathLengh)
                        canBuildPath = false;

                    if (IsSnappingAngle && CanSnap(hitObject))
                    {
                        // if we hit ground or a path
                        hitPosition = PathUtilities.GetHitPositionWithSnapping(hitPosition, startNode, angleToSnap);
                    }
                }

                nodeGFX.transform.position = hitPosition;
                buildingState.UpdateState(hitPosition, pathSO, canBuildPath);
            }
        }

        private bool ValidateAngle(GameObject hitObject, Vector3 hitPosition)
        {
            if (CheckPathAngleInRange(StartPosition - ControlPosition))
                return CheckPathAngleInRange(ControlPosition - hitPosition, hitObject, hitPosition);

            return false;
        }

        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e)
        {
            Vector3 hitPosition = e.position;
            if (startNode != null && IsSnappingAngle && CanSnap(e.obj))
            {
                // if we hit ground or a path
                hitPosition = PathUtilities.GetHitPositionWithSnapping(hitPosition, startNode, angleToSnap);
            }

            hitPosition = GetPositionForMinPathLengh(hitPosition);
            buildingState.OnAction(hitPosition, canBuildPath);
        }

        private void PathUIController_OnPathDown() {
            throw new NotImplementedException();
        }

        private void PathUIController_OnPathUp() {
            throw new NotImplementedException();
        }

        private void PathUIController_OnAngleSnapping() {
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

        private void PathUIController_OnGridSnapping() {
            throw new NotImplementedException();
        }

        public bool CheckPathAngleInRange(Vector3 pathDirection) {
            if (startNode == null && pathsToSplit.Count <= 0) 
                return true;

            float angle;
            float firstAngle; 
            float secondAngle;

            foreach (PathObject pathObject in startNode.ConnectedPaths)
            {
                Vector3 prevPathDirection = startNode.Position - pathObject.ControlPosition;
                angle = Vector3.Angle(prevPathDirection, pathDirection);
                angle = Mathf.Clamp(angle, 0, 90);

                if (angle < minAllowedAngle)
                    return false;;
            }

            if (pathsToSplit.Count > 0) {
                PathObject pathObject = pathsToSplit.First().Value;
                Vector3 hitPosition = pathsToSplit.First().Key;

                Vector3 nextPathDirection = Bezier.GetTangentAt(pathObject, hitPosition, out _, out _);
                firstAngle = Vector3.Angle(nextPathDirection, pathDirection);
                secondAngle = 180 - firstAngle;
                firstAngle = Mathf.Clamp(firstAngle, 0, 90);
                secondAngle = Mathf.Clamp(secondAngle, 0, 90);

                if (firstAngle < minAllowedAngle || secondAngle < minAllowedAngle) 
                    return false;
            }

            return true;
        }

        public bool CheckPathAngleInRange(Vector3 pathDirection, GameObject hitObj, Vector3 hitPosition) 
        {            
            if (hitObj.TryGetComponent(out NodeObject node)) 
            {
                foreach (PathObject pathObj in node.ConnectedPaths) 
                {
                    Vector3 nextPathDirection = pathObj.ControlPosition - node.Position;
                    float angle = Vector3.Angle(nextPathDirection, pathDirection);
                    if (angle < minAllowedAngle) 
                        return false;
                }
            }                

            if (hitObj.TryGetComponent(out PathObject pathObject)) 
            {
                Vector3 nextPathDirection = Bezier.GetTangentAt(pathObject, hitPosition, out _, out _);

                float firstAngle = Vector3.Angle(nextPathDirection, pathDirection);
                float secondAngle = 180 - firstAngle;
                firstAngle = Mathf.Clamp(firstAngle, 0, 90);
                secondAngle = Mathf.Clamp(secondAngle, 0, 90);

                if (firstAngle < minAllowedAngle || secondAngle < minAllowedAngle)
                    return false;
            }

            return true;
        }

        private void UIController_OnRemovingObjects() {
            ResetBuildingState();
        }

        private void InputManager_OnEscape()
        {
            ResetBuildingState();
        }

        private void PathUIController_OnBuildingStraightPath(PathSO pathObjectSO) {
            ResetBuildingState();
            this.pathSO = pathObjectSO;
            minPathLengh = pathObjectSO.width * 1.5f;
            if (nodeGFX == null) nodeGFX = PathUtilities.CreateNodeGFX(pathObjectSO);
            else nodeGFX.SetActive(true);

            buildingState = new BuildingStraightPath(this);
        }

        private void PathUIController_OnBuildingCurvedPath(PathSO pathObjectSO) {
            ResetBuildingState();
            this.pathSO = pathObjectSO;
            minPathLengh = pathObjectSO.width * 1.5f;
            if (nodeGFX == null) nodeGFX = PathUtilities.CreateNodeGFX(pathObjectSO);
            else nodeGFX.SetActive(true);

            buildingState = new BuildingCurvedPath(this);
        }

        private void PathUIController_OnBuildingFreePath(PathSO pathObjectSO) {
            ResetBuildingState();
            this.pathSO = pathObjectSO;
            minPathLengh = pathObjectSO.width * 1.5f;
            if (nodeGFX == null) nodeGFX = PathUtilities.CreateNodeGFX(pathObjectSO);
            else nodeGFX.SetActive(true);

            buildingState = new BuildingFreePath(this);
        }

        private void ResetBuildingState()
        {
            ClearAffectedPath();
            ResetDisplayPath();
            buildingState = null;
            nodeBuildingState = NodeBuildingState.StartNode;
            if (nodeGFX != null) nodeGFX.SetActive(false);
        }

        public Vector3 GetPositionForMinPathLengh(Vector3 position) {
            if (startNode != null) {
                Vector3 pathDir = position - startNode.Position;

                if (pathDir.magnitude < minPathLengh)
                    position += pathDir.normalized * minPathLengh - pathDir;
            }

            return position;
        }
        public bool IsBuildingStartNode() => nodeBuildingState == NodeBuildingState.StartNode;
        public bool IsBuildingControlNode() => nodeBuildingState == NodeBuildingState.ControlNode;
        public bool IsBuildingEndNode() => nodeBuildingState == NodeBuildingState.EndNode;
        
        public NodeObject StartNode { get { return startNode; } }
        public Vector3 StartPosition { 
            get { return startNode.Position; } 
            set { startNode = pathManager.GetOrCreateNodeAt(value); } 
        }
        public Vector3 EndPosition
        {
            get { return endNode.Position; }
            set { endNode = pathManager.GetOrCreateNodeAt(value); }
        }
        public Vector3 ControlPosition
        {
            get { return controlPosition; }
            set { controlPosition = value; }
        }
        public bool IsSnappingAngle => snappingAngle != AngleSnap.Zero;

        private void ResetDisplayPath() {
            ResetPathPositions();
            canBuildPath = true;
            buildingState?.StopPreviewDisplay();
        }
        private void ResetPathPositions()
        {
            if (startNode != null && !startNode.HasConnectedPaths)
            {
                // pathManager.RemoveNode(startNode);
                Destroy(startNode.gameObject);
            }

            if (endNode != null && !endNode.HasConnectedPaths)
            {
                // pathManager.RemoveNode(endNode);
                Destroy(endNode.gameObject);
            }

            endNode = null;
            startNode = null;
            controlPosition = Vector3.negativeInfinity;
        }

        private void InputManager_OnCancel() {
            nodeBuildingState = NodeBuildingState.StartNode;
            ClearAffectedPath();
            ResetDisplayPath();
        }

        public void PlacePath() {
            PathObject placedPath = pathSO.CreatePathObject(startNode, endNode, controlPosition, pathManager.PathParentTransform);
            NodeObject cachedEndNode = endNode;
            ResetPathPositions();
            startNode = cachedEndNode;

            OnPathPlaced?.Invoke(this, new OnPathPlacedEventArgs { pathObject = placedPath });
        }

        public void SplitPath() {
            foreach (Vector3 positionToSplit in pathsToSplit.Keys) {
                PathObject pathToSplit = pathsToSplit[positionToSplit];
                NodeObject intersectionNode = pathManager.GetNodeAt(positionToSplit);
                Bezier.GetTangentAt(
                    pathToSplit,
                    intersectionNode.Position,
                    out Vector3 startControlPosition,
                    out Vector3 endControlPosition);

                Vector3 startNodePosition = pathToSplit.StartNode.Position;
                Vector3 endNodePosition = pathToSplit.EndNode.Position;

                pathToSplit.RemovePath();

                NodeObject startNode = pathManager.GetOrCreateNodeAt(startNodePosition);
                NodeObject endNode = pathManager.GetOrCreateNodeAt(endNodePosition);

                pathToSplit.PathSO.SplitPathObject(
                    startNode,
                    endNode,
                    intersectionNode,
                    startControlPosition,
                    endControlPosition,
                    pathManager.PathParentTransform);
            }
            pathsToSplit.Clear();
        }

        private void ClearAffectedPath() {
            pathsToSplit.Clear();
        }

        public void AddPathToSplit(Vector3 position, PathObject pathObject) {
            if (!pathsToSplit.ContainsKey(position)) {
                pathsToSplit.Add(position, pathObject);
            }
        }

        public bool CanSnap(GameObject hitObject) {
            if (hitObject.TryGetComponent(out Ground _) || hitObject.TryGetComponent(out PathObject _))
                return true;
            return false;
        }

        public void UpdateBuildingState(NodeBuildingState state)
        {
            nodeBuildingState = state;
        }
    }
}