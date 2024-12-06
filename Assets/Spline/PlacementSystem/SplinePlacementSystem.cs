using UnityEngine;
using System;
using System.Collections.Generic;
using Global.UI;
using World;
using Rafael.Utils;
using Spline.UI;
using Spline.Entities;
using Path.Utilities;

namespace Spline.Placement {

    public class SplinePlacementSystem : MonoBehaviour
    {
        public static SplinePlacementSystem Instance { get; private set; }

        public enum NodeBuildingState {
            StartNode,
            ControlNode,
            EndNode,
        }

        private readonly Dictionary<Vector3, Segment> pathsToSplit = new();

        private SplineUIController splineUIController;
        private InputManager inputManager;
        private UIController uIController;
        private SplineManager splineManager;

        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private GameObject segmentPrefab;

        [SerializeField] private Transform segmentParentTransform;
        [SerializeField] private Transform nodeParentTransform;
        public float controlNodeRatio { get; private set; }
        public int splineResolution { get; private set; }

        private Node startNode;
        private Node endNode;
        private GameObject nodeGFX;

        private float minPathLengh;
        private bool canBuildPath;
        private Vector3 controlPosition;

        private IPlacementState buildingState;
        private NodeBuildingState nodeBuildingState;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            splineUIController = SplineUIController.Instance;
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;
            splineManager = SplineManager.Instance;

            controlNodeRatio = 0.55f;
            splineResolution = 12;

            canBuildPath = true;
            minPathLengh = 20f;

            splineUIController.OnStraightModeSelected += SplineUIController_OnStraightModeSelected;
            splineUIController.OnCurvedModeSelected += SplineUIController_OnCurvedModeSelected;
            splineUIController.OnFreeModeSelected += SplineUIController_OnFreeModeSelected;

            splineUIController.OnGridSnapping += SplineUIController_OnGridSnapping;
            splineUIController.OnAngleSnapping += SplineUIController_OnAngleSnapping;

            splineUIController.OnPathUp += SplineUIController_OnPathUp;
            splineUIController.OnPathDown += SplineUIController_OnPathDown;

            splineUIController.OnObjectToBuildSelected += SplineUIController_OnObjectToBuildSelected;

            uIController.OnRemovingObjects += UIController_OnRemovingObjects;

            inputManager.OnEscape += InputManager_OnEscape;
            inputManager.OnCancel += InputManager_OnCancel;
            inputManager.OnNodePlaced += InputManager_OnNodePlaced;

            nodeGFX = PathUtilities.UpdateOrCreateNodeGFX(1.1f, nodeGFX);
            nodeGFX.SetActive(false);
        }

        private void Update()
        {
            if (buildingState == null) return;

            Vector3 position = HandlePathPositioning(out _);

            nodeGFX.transform.position = position;
            buildingState.UpdateState(position, canBuildPath);           
        }
        private void InputManager_OnCancel()
        {
            Debug.Log("Node Building State: Start Node");
            nodeBuildingState = NodeBuildingState.StartNode;
            ResetDisplayPath();
        }
        private void InputManager_OnEscape()
        {
            ResetBuildingState();
        }
        private void InputManager_OnNodePlaced()
        {
            if (buildingState == null)
            {
                Debug.LogError("Trying to place node without a building state!");
                return;
            }
                
            Vector3 position = HandlePathPositioning(out GameObject hitObject);
            //if (hitObject.TryGetComponent(out Segment segment))
            //{
            //    AddPathToSplit(position, segment);
            //}

            buildingState.OnAction(position, canBuildPath);

        }
        private void SplineUIController_OnPathDown() {
            throw new NotImplementedException();
        }
        private void SplineUIController_OnPathUp() {
            throw new NotImplementedException();
        }
        private void SplineUIController_OnAngleSnapping()
        {
            throw new NotImplementedException();
        }
        private void SplineUIController_OnGridSnapping() {
            throw new NotImplementedException();
        }
        private void SplineUIController_OnStraightModeSelected() {
            ResetBuildingState();
            Debug.Log("Building State: Straight Spline");
            nodeGFX.SetActive(true);
            buildingState = new BuildingStraightSpline(this);
        }
        private void SplineUIController_OnCurvedModeSelected() {
            ResetBuildingState();
            Debug.Log("Building State: Curved Spline");
            nodeGFX.SetActive(true);
            buildingState = new BuildingCurvedSpline(this);
        }
        private void SplineUIController_OnFreeModeSelected()
        {
            ResetBuildingState();
            Debug.Log("Building State: Free Spline");
            nodeGFX.SetActive(true);
            buildingState = new BuildingFreeSpline(this);
        }
        private void SplineUIController_OnObjectToBuildSelected()
        {
            ResetBuildingState();
            Debug.Log("Building State: Straight Spline");
            nodeGFX.SetActive(true);

            buildingState = new BuildingStraightSpline(this);
        }
        public void PlacePath()
        { 
            Segment segment = Instantiate(
                segmentPrefab,
                controlPosition,
                Quaternion.identity,
                segmentParentTransform).GetComponent<Segment>();

            segment.Init(startNode, endNode, controlPosition, controlNodeRatio);

            Node cachedEndNode = endNode;
            ResetPathPositions();
            startNode = cachedEndNode;
        }
        private void ResetBuildingState()
        {
            Debug.Log("Building State: None");
            Debug.Log("Node Building State: Start Node");
            ResetDisplayPath();
            buildingState = null;
            nodeBuildingState = NodeBuildingState.StartNode;
            if (nodeGFX != null) nodeGFX.SetActive(false);
        }
        private void ResetDisplayPath()
        {
            ResetPathPositions();
            canBuildPath = true;
            pathsToSplit.Clear();
            buildingState?.StopPreviewDisplay();
        }
        private void ResetPathPositions()
        {
            if (startNode != null && !startNode.HasConnection)
                splineManager.RemoveNode(startNode);

            if (endNode != null && !endNode.HasConnection)
                splineManager.RemoveNode(endNode);

            endNode = null;
            startNode = null;
            controlPosition = Vector3.negativeInfinity;
        }
        private void UIController_OnRemovingObjects()
        {
            ResetBuildingState();
        }
        //public void SplitPath() {
        //    foreach (Vector3 positionToSplit in pathsToSplit.Keys) {
        //        Segment segmentToSplit = pathsToSplit[positionToSplit];
        //        Node intersectionNode = pathManager.GetNodeAt(positionToSplit);
        //        Bezier.GetTangentAt(
        //            segmentToSplit,
        //            intersectionNode.position,
        //            out Vector3 startControlPosition,
        //            out Vector3 endControlPosition);

        //        Vector3 startNodePosition = segmentToSplit.StartNode.Position;
        //        Vector3 endNodePosition = segmentToSplit.EndNode.Position;

        //        PathSO pathToSplitSO = segmentToSplit.PathSO;
        //        segmentToSplit.RemovePath();

        //        Node startNode = pathManager.GetOrCreateNodeAt(
        //            startNodePosition,
        //            pathToSplitSO.pathObjectPrefab.GetComponent<Segment>());
        //        Node endNode = pathManager.GetOrCreateNodeAt(
        //            endNodePosition, 
        //            pathToSplitSO.pathObjectPrefab.GetComponent<Segment>());

        //        pathToSplitSO.SplitPathObject(
        //            startNode,
        //            endNode,
        //            intersectionNode,
        //            startControlPosition,
        //            endControlPosition,
        //            pathManager.SegmentParentTransform, 
        //            out Segment firstPlacedPath,
        //            out Segment secondPlacedPath);
        //            pathManager.AddPath(firstPlacedPath);
        //            pathManager.AddPath(secondPlacedPath);    
        //    }
        //    pathsToSplit.Clear();
        //}
        private void AddPathToSplit(Vector3 position, Segment pathObject) {
            if (!pathsToSplit.ContainsKey(position)) {
                pathsToSplit.Add(position, pathObject);
            }
        }

        public NodeBuildingState BuildingState
        {
            get { return nodeBuildingState; }
            set {
                Debug.Log("Node Building State: " + value);
                nodeBuildingState = value; 
            }
        }
        public Vector3 GetPositionForMinPathLengh(Vector3 position)
        {
            Vector3 pathDir = position - startNode.position;
            if (pathDir.magnitude < minPathLengh)
                position += pathDir.normalized * minPathLengh - pathDir;

            return position;
        }
        public Node StartNode { get { return startNode; } }
        public Vector3 StartPosition { 
            get { return startNode.position; }
            set { startNode = GetOrCreateNodeAt(value); } 
        }
        public Vector3 EndPosition
        {
            get { return endNode.position; }
            set { endNode = GetOrCreateNodeAt(value); }
        }
        public Vector3 ControlPosition
        {
            get { return controlPosition; }
            set { controlPosition = value; }
        }
        private Vector3 HandlePathPositioning(out GameObject hitObject)
        {
            hitObject = null;
            if (RafaelUtils.TryRaycastObjectt(out Vector3 hitPosition, out Ground ground, 1))
            {
                hitPosition = new Vector3(hitPosition.x, hitPosition.y + 0.1f, hitPosition.z);
                hitObject = ground.gameObject;
                return hitPosition;
            }
            return Vector3.zero;
        }
        public bool IsBuildingStartNode() => nodeBuildingState == NodeBuildingState.StartNode;
        public bool IsBuildingControlNode() => nodeBuildingState == NodeBuildingState.ControlNode;
        public bool IsBuildingEndNode() => nodeBuildingState == NodeBuildingState.EndNode;

        public Node GetOrCreateNodeAt(Vector3 position)
        {
            Node node = splineManager.TryGetNodeAt(position);
            if (node != null)
                return node;

            node = Instantiate(
                nodePrefab, 
                position, 
                Quaternion.identity, 
                nodeParentTransform).GetComponent<Node>();

            splineManager.AddNode(node);
            Debug.Log(node);
            return node;
        }
    }
}