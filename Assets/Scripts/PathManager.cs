using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PathManager : MonoBehaviour
{
    [HideInInspector]
    List<RoadSegmentObject> path = new List<RoadSegmentObject>();

    PathCreator pathCreator;
    RoadUIController uiController;
    InputManager inputManager;

    bool placingPoints = false;
    bool finishedPlacingPoints = false;
    Node startNode;
    Node endNode;
    RoadSegmentObject firstRoadToSplit;
    RoadSegmentObject secondRoadToSplit;

    GameObject nodeGFX;
    RoadSegment roadSegmentSO;
    [SerializeField]
    List<RoadSegmentObject> roadSegmentToRecheck = new List<RoadSegmentObject>();

    bool canBuildRoad = false;
    bool isStartPosANode = false;

    [SerializeField]
    bool gridSnap = false;
    int gridCellSize;

    [SerializeField]
    float minAngleBetweenRoads = 15;

    [SerializeField]
    bool angleSnap = false;
    [SerializeField]
    float snapAngle = 5;

    Vector3 startPoint;
    Vector3 endPoint;
    MeshFilter displayMesh;
    MeshRenderer displayMeshRenderer;
    public Material canBuildMaterial;
    public Material cantBuildMaterial;


    void Start()
    {
        pathCreator = GetComponent<PathCreator>();
        uiController = FindObjectOfType<RoadUIController>();
        inputManager = FindObjectOfType<InputManager>();
        displayMesh = GetComponent<MeshFilter>();
        displayMeshRenderer = GetComponent<MeshRenderer>();

        //uiController.OnStraightRoadPlacement += StraightRoadHandler;
        //uiController.OnCurveRoadPlacement += ;
        //uiController.OnFreePlacement += ;
    }

    void Update()
    {
    }

    //float GetAngleWithRoad(RoadObject existingRoad, Vector3 pointOfContact, Vector3 incindingVector)
    //{
    //    Vector3 tangent = Bezier.GetTangent(existingRoad.ControlNode.transform.position, pointOfContact, pointOfContact);
    //    float angle = Vector3.Angle(tangent, incindingVector);

    //    return angle;
    //}

    //private void HandleTemporaryPlacement(Vector3 position, GameObject obj)
    //{
    //    if (nodeGFX == null) return;
    //    DisplayTemporaryMesh();
    //    canBuildRoad = true;

    //    Vector3 roadDir = (startPoint - position).normalized;
    //    Vector3 nodePosition = position;

    //    foreach (RoadObject segment in roadSegmentToRecheck)
    //    {

    //        float angle = GetAngleWithRoad(segment, startPoint, roadDir);

    //        if (
    //            angle < minAngleBetweenRoads ||
    //            (angle > 180 - minAngleBetweenRoads && !isStartPosANode)
    //        )
    //        {
    //            canBuildRoad = false;
    //        }
    //    }
    //    if (obj.CompareTag("Ground"))
    //    {
    //        if (gridSnap)
    //        {
    //            if (Vector3.Distance(nodeGFX.transform.position, position) > gridCellSize)
    //            {
    //                nodePosition = new Vector3(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
    //                print(nodePosition);
    //            }
    //        }
            
    //        if (angleSnap)
    //        {
    //            float roadLength = (position - startPoint).magnitude;
    //            foreach (RoadSegmentObject segment in roadSegmentToRecheck)
    //            {
    //                Vector3 segmentDir = (startPoint - segment.ControlPointPosition()).normalized;

    //                Debug.DrawLine(startPoint, segment.ControlPointPosition(), Color.cyan, 1);

    //                float angle = GetAngleWithRoad(segment, startPoint, roadDir);
    //                print(angle);
    //                if (angle <= snapAngle)
    //                {
    //                    nodePosition = -(roadLength * segmentDir) + startPoint;
    //                }
    //                else if(angle >= 180 - snapAngle)
    //                {
    //                    nodePosition = roadLength * segmentDir + startPoint;
    //                }
    //                else if(angle <= 90 + snapAngle && angle >= 90 - snapAngle)
    //                {
    //                    float dot = Vector3.Cross(segmentDir, roadDir).y;
    //                    segmentDir = new Vector3(-segmentDir.z, segmentDir.y, segmentDir.x);
    //                    segmentDir = segmentDir * Mathf.Sign(dot);
    //                    nodePosition = roadLength * segmentDir + startPoint;
    //                    print(dot);
    //                }
    //                //else if(angle >= 90 - snapAngle)
    //                //{
    //                //    nodePosition = roadLength * segmentDir + startPoint;
    //                //    nodePosition = new Vector3(-nodePosition.z, nodePosition.y, nodePosition.x) * -1;
    //                //}
    //            }
    //        }
                
    //        nodeGFX.transform.position = nodePosition;
    //    }
    //    if (obj.CompareTag("Node"))
    //    {
    //        nodeGFX.transform.position = obj.transform.position;
    //    }
    //    if (obj.CompareTag("RoadSegment"))
    //    {
    //        RoadObject segment = obj.GetComponent<RoadObject>();
    //        nodePosition = Bezier.GetClosestPointTo(position, segment);
    //        nodeGFX.transform.position = nodePosition;
    //    }
    //}

    //void CreateStraightRoad(GameObject obj)
    //{
    //    if (!canBuildRoad) return;
    //    RoadObject segment;
    //    Vector3 point = nodeGFX.transform.position;

    //    if (placingPoints && !finishedPlacingPoints)
    //    {            
    //        if (obj.CompareTag("Node"))
    //        {
    //            endNode = obj.GetComponent<Node>();
    //            roadSegmentToRecheck.AddRange(endNode.GetConnectedRoads());
    //            endPoint = obj.transform.position;
    //        }
    //        else if (obj.CompareTag("RoadSegment"))
    //        {
    //            segment = obj.GetComponent<RoadObject>();
    //            Vector3 pos = Bezier.GetClosestPointTo(point, segment);
    //            endPoint = pos;

    //            secondRoadToSplit = secondRoadToSplit = segment;
    //            roadSegmentToRecheck.Add(segment);
    //        }
    //        else
    //        {
    //            endPoint = point;
    //        }

    //        placingPoints = false;
    //        finishedPlacingPoints = true;
    //    }
    //    else
    //    {
    //        if (obj.CompareTag("Node"))
    //        {
    //            startNode = obj.GetComponent<Node>();
    //            roadSegmentToRecheck.AddRange(startNode.GetConnectedRoads());
    //            startPoint = obj.transform.position;
    //            isStartPosANode = true;
    //        }
    //        else if (obj.CompareTag("RoadSegment"))
    //        {
    //            segment = obj.GetComponent<RoadObject>();
    //            Vector3 pos = Bezier.GetClosestPointTo(point, segment);
    //            startPoint = pos;

    //            firstRoadToSplit = segment;
    //            roadSegmentToRecheck.Add(segment);
    //            isStartPosANode = false;
    //        }
    //        else
    //        {
    //            startPoint = point;
    //            isStartPosANode = false;
    //        }
    //        placingPoints = true;
    //    }

    //    if (!finishedPlacingPoints) return;

    //    print("Placing Road");

    //    if (firstRoadToSplit != null)
    //        startNode = pathCreator.SplitSegment(firstRoadToSplit, startPoint);
    //    if (secondRoadToSplit!= null)
    //        endNode = pathCreator.SplitSegment(secondRoadToSplit, endPoint);

    //    if (startNode == null && endNode == null)
    //        pathCreator.CreateSegment(startPoint, endPoint, roadSegmentSO);
    //    else if (startNode != null && endNode != null)
    //        pathCreator.CreateSegment(startNode.transform.gameObject, endNode.transform.gameObject, roadSegmentSO);
    //    else if (startNode != null && endNode == null)
    //        pathCreator.CreateSegment(startNode.transform.gameObject, endPoint, roadSegmentSO);
    //    else if (endNode != null && startNode == null)
    //        pathCreator.CreateSegment(startPoint, endNode.transform.gameObject, roadSegmentSO);


    //    firstRoadToSplit = null;
    //    secondRoadToSplit = null;
    //    startNode = null;
    //    endNode = null;
    //    finishedPlacingPoints = false;
    //    roadSegmentToRecheck.Clear();
    //    print("Road Created!");
    //}

}
