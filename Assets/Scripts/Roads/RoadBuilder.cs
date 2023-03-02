using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadBuilder : MonoBehaviour
{
    [SerializeField] private Transform roadParentTransform;
    [SerializeField] private Transform nodeParentTransform;
    [SerializeField] private GameObject node;

    public static RoadBuilder Instance { get; private set; }
    private void Awake() {
        Instance = this;
    }

    public RoadObject CreateRoadSegment(Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, RoadObjectSO roadObjectSO) {
        GameObject startNode = Instantiate(node, startPosition, Quaternion.identity, nodeParentTransform);
        GameObject endNode = Instantiate(node, endPosition, Quaternion.identity, nodeParentTransform);
        return BuildRoad(startNode.GetComponent<Node>(), endNode.GetComponent<Node>(), controlPosition, roadObjectSO);        
    }

    public RoadObject CreateRoadSegment(Node startNode, Vector3 endPosition, Vector3 controlPosition, RoadObjectSO roadObjectSO) {        
        GameObject endNode = Instantiate(node, endPosition, Quaternion.identity, nodeParentTransform);
        return BuildRoad(startNode, endNode.GetComponent<Node>(), controlPosition, roadObjectSO);        
    }

    public RoadObject CreateRoadSegment(Vector3 startPosition, Node endNode, Vector3 controlPosition, RoadObjectSO roadObjectSO) {
        GameObject startNode = Instantiate(node, startPosition, Quaternion.identity, nodeParentTransform);
        return BuildRoad(startNode.GetComponent<Node>(), endNode, controlPosition, roadObjectSO);
        
    }

    public RoadObject CreateRoadSegment(Node startNode, Node endNode, Vector3 controlPosition, RoadObjectSO roadObjectSO) {
        return BuildRoad(startNode, endNode, controlPosition, roadObjectSO);        
    }

    private RoadObject BuildRoad(Node startNode, Node endNode, Vector3 controlPosition, RoadObjectSO roadObjectSO) {
        Vector3 roadPosition = (startNode.gameObject.transform.position + endNode.gameObject.transform.position) / 2;
        GameObject roadGameObject = Instantiate(roadObjectSO.roadObjectPrefab, roadPosition, Quaternion.identity, roadParentTransform);
        GameObject controlNodeObject = CreateControlNode(roadObjectSO, controlPosition);
        RoadObject roadObject = roadGameObject.GetComponent<RoadObject>();

        roadObject.Init(startNode, endNode, controlNodeObject);
        Mesh roadMesh = RoadMeshBuilder.Instance.CreateRoadMesh(roadObject);

        roadObject.SetRoadMesh(roadMesh);
        return roadObject;
    }

    public Node SplitRoadSegment(RoadObject roadObject, RoadObjectSO roadObjectSO, Node newNode) {
        Node startNode = roadObject.StartNode;
        Node endNode = roadObject.EndNode;

        Bezier.GetTangentAt(
            roadObject,
            newNode.Position,
            out Vector3 newStartControlPointPosition,
            out Vector3 newEndControlPointPosition);

        RemoveSegment(roadObject);

        CreateRoadSegment(startNode, newNode, newStartControlPointPosition, roadObjectSO);
        CreateRoadSegment(newNode, endNode, newEndControlPointPosition, roadObjectSO);


        return newNode;
    }

    public void RemoveSegment(RoadObject roadObject) {
        roadObject.StartNode.RemoveRoadSegment(roadObject);
        roadObject.EndNode.RemoveRoadSegment(roadObject);
        Destroy(roadObject.gameObject);
    }

    private GameObject CreateControlNode(RoadObjectSO roadObjectSO, Vector3 controlNodePosition) {
        GameObject controlNodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        controlNodeObject.transform.localScale = 0.25f * roadObjectSO.roadWidth * Vector3.one;
        controlNodeObject.transform.position = controlNodePosition;
        controlNodeObject.transform.parent = this.transform;
        return controlNodeObject;
    }

}

