using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using MeshHandler.Road;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadPlacement : MonoBehaviour {
    private enum State {
        Idle,
        StraightRoad,
        CurvedRoad,
        RemovingRoad,
    }

    private enum BuildingState {
        StartNode,
        ControlNode,
        EndNode,
    }

    [SerializeField] private Material temporaryRoadMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private GameManager gameManager;
    private RoadUIController roadUIController;
    private RoadBuilder roadBuilder;

    private RoadObjectSO roadObjectSO;
    private GameObject nodeGFX;

    private State state;
    private BuildingState buildingState;

    private Node startNode;
    private Node endNode;

    private Vector3 startNodePosition;
    private Vector3 controlNodePosition;
    private Vector3 endNodePosition;

    private Dictionary<Vector3, RoadObject> roadsToSplit;
    private List<RoadObject> roadsToUpdate;

    private void Start() {
        gameManager = GameManager.Instance;
        roadUIController = RoadUIController.Instance;
        roadBuilder = RoadBuilder.Instance;

        roadUIController.OnStraightRoadPlacement += RoadUIController_OnStraightRoadPlacement;
        roadUIController.OnCurveRoadPlacement += RoadUiController_OnCurvedRoadPlacement;
        roadUIController.OnRoadRemoved += RoadUIController_OnRoadRemoved;

        state = State.Idle;
        buildingState = BuildingState.StartNode;

        roadsToSplit = new Dictionary<Vector3, RoadObject>();
        roadsToUpdate = new List<RoadObject>();

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

    }

    private void RoadUIController_OnRoadRemoved() {
        ResetBuildingState();
        InputManager.Instance.OnMouseClick += RemoveRoad;
        state = State.RemovingRoad;
    }

    private void RemoveRoad(GameObject obj) {
        if (obj.TryGetComponent(out RoadObject roadObject)) {
            roadBuilder.RemoveSegment(roadObject);
            roadsToUpdate.AddRange(roadObject.StartNode.GetConnectedRoads());
            roadsToUpdate.AddRange(roadObject.EndNode.GetConnectedRoads());
        }
        print("roads destroyed");
        UpdateRoads();
    }

    private void RoadUiController_OnCurvedRoadPlacement(RoadObjectSO roadObjectSO) {
        ResetBuildingState();
        this.roadObjectSO = roadObjectSO;
        state = State.CurvedRoad;
    }

    private void RoadUIController_OnStraightRoadPlacement(RoadObjectSO roadObjectSO) {
        ResetBuildingState();
        this.roadObjectSO = roadObjectSO;
        state = State.StraightRoad;
    }
    private void Update() {
        if (!gameManager.IsBuilding() || !IsBuilding()) return;
        if (CheckRightMouseButtonClick()) ResetBuildingState();

        Vector3 hitPosition;
        GameObject hitGameObject = RayCastObject(out hitPosition);
        if (hitGameObject == null) return;

        if (nodeGFX == null) CreateNodeGfx();

        if (hitGameObject.TryGetComponent(out RoadObject roadObject)) {
            Vector3 roadPostion = Bezier.GetClosestPointTo(roadObject, hitPosition);
            nodeGFX.transform.position = roadPostion;
        } else if (hitGameObject.TryGetComponent(out Node node)) {
            nodeGFX.transform.position = node.gameObject.transform.position;
        } else {
            nodeGFX.transform.position = new Vector3(
                hitPosition.x,
                hitPosition.y + 0.1f,
                hitPosition.z
            );
        }

        switch (buildingState) {
            default:
            case BuildingState.StartNode:
                if (CheckLeftMouseButtonClick()) {
                    if (hitGameObject.TryGetComponent(out Node startNode)) {
                        this.startNode = startNode;
                    }
                    else if(hitGameObject.TryGetComponent(out roadObject)) {
                        roadsToSplit.Add(nodeGFX.transform.position, roadObject);
                    }
                    startNodePosition = nodeGFX.transform.position;
                    if (state == State.StraightRoad) buildingState = BuildingState.EndNode;
                    else if (state == State.CurvedRoad) buildingState = BuildingState.ControlNode;
                }
                break;
            case BuildingState.ControlNode:
                controlNodePosition = nodeGFX.transform.position;
                if (CheckLeftMouseButtonClick()) {
                    buildingState = BuildingState.EndNode;
                    break;
                }
                Vector3 temporaryControlNodePosition = (startNodePosition + controlNodePosition) / 2;
                DisplayTemporaryMesh(startNodePosition, controlNodePosition, temporaryControlNodePosition);
                break;
            case BuildingState.EndNode:
                endNodePosition = nodeGFX.transform.position;
                if (state == State.StraightRoad) {
                    controlNodePosition = (startNodePosition + endNodePosition) / 2;
                }
                DisplayTemporaryMesh(startNodePosition, endNodePosition, controlNodePosition);        
                if (CheckLeftMouseButtonClick()) {
                    if (hitGameObject.TryGetComponent(out Node endNode)) {
                        this.endNode = endNode;
                    } 
                    else if (hitGameObject.TryGetComponent(out roadObject)) {
                        roadsToSplit.Add(nodeGFX.transform.position, roadObject);
                    }
                    CreateRoadSegment();
                    startNodePosition = endNodePosition;
                    if (state == State.CurvedRoad) buildingState = BuildingState.ControlNode;
                }
                break;
        }
    }


    private void CreateRoadSegment() {
        RoadObject newRoadObject;

        if (startNode == null && endNode == null)
            newRoadObject = roadBuilder.CreateRoadSegment(startNodePosition, endNodePosition, controlNodePosition, roadObjectSO);
        else if (startNode != null && endNode == null) {
            newRoadObject = roadBuilder.CreateRoadSegment(startNode, endNodePosition, controlNodePosition, roadObjectSO);
            roadsToUpdate.AddRange(startNode.GetConnectedRoads());
        } else if (startNode == null && endNode != null) {
            newRoadObject = roadBuilder.CreateRoadSegment(startNodePosition, endNode, controlNodePosition, roadObjectSO);
            roadsToUpdate.AddRange(endNode.GetConnectedRoads());
        } else {
            newRoadObject = roadBuilder.CreateRoadSegment(startNode, endNode, controlNodePosition, roadObjectSO);
            roadsToUpdate.AddRange(startNode.GetConnectedRoads());
            roadsToUpdate.AddRange(endNode.GetConnectedRoads());
        }

        startNode = newRoadObject.EndNode;

        SplitRoads(newRoadObject);
        roadsToUpdate.Add(newRoadObject);
        UpdateRoads();
    }

    private void UpdateRoads() {
        foreach (RoadObject roadToUpdate in roadsToUpdate) {
            Mesh roadMesh = RoadMeshBuilder.Instance.CreateRoadMesh(roadToUpdate);
            roadToUpdate.SetRoadMesh(roadMesh);
        }
        roadsToUpdate.Clear();
    }

    private void SplitRoads(RoadObject newRoadObject) {
        if (roadsToSplit.Count > 0) {
            foreach (Vector3 positionToSplit in roadsToSplit.Keys) {
                RoadObject roadObject = roadsToSplit[positionToSplit];
                Node centerNode;

                if (newRoadObject.StartNode.Position == positionToSplit)
                    centerNode = newRoadObject.StartNode;
                else 
                    centerNode = newRoadObject.EndNode;
                roadBuilder.SplitRoadSegment(roadObject, roadObjectSO, centerNode);                
            }
        }
        roadsToSplit.Clear();
    }

    private void CreateNodeGfx() {
        nodeGFX = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nodeGFX.transform.GetComponent<Collider>().enabled = false;
        nodeGFX.transform.localScale = roadObjectSO.roadWidth * Vector3.one;
    }

    private GameObject RayCastObject(out Vector3 hitPosition) {
        hitPosition = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {
            hitPosition = hit.point;
            return hit.collider.gameObject;
        }
        return null;
    }

    public bool IsBuilding() {
        return state != State.Idle && state != State.RemovingRoad;
    }

    void DisplayTemporaryMesh(Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition) {
        Mesh mesh = RoadMeshBuilder.Instance.CreateRoadMesh(roadObjectSO.roadWidth, startPosition, endPosition, controlPosition);
        meshRenderer.sharedMaterial = temporaryRoadMaterial;
        meshFilter.mesh = mesh;
    }

    private void ResetBuildingState() {
        startNodePosition = Vector3.zero;
        controlNodePosition = Vector3.zero;
        endNodePosition = Vector3.zero;
        meshFilter.mesh = null;
        startNode = null;
        endNode = null;
        roadsToSplit.Clear();
        roadsToUpdate.Clear();
        buildingState = BuildingState.StartNode;
        InputManager.Instance.OnMouseClick -= RemoveRoad;

    }

    private bool CheckLeftMouseButtonClick() {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false) {
            return true;
        }
        return false;
    }

    private bool CheckRightMouseButtonClick() {
        if (Input.GetMouseButtonDown(1) && EventSystem.current.IsPointerOverGameObject() == false) {
            return true;
        }
        return false;
    }
}
