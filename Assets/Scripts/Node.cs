using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rafael.utils;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(SphereCollider))]
public class Node : MonoBehaviour
{
    [SerializeField]
    List<RoadObject> connectedRoads = new List<RoadObject>();

    [SerializeField]
    int resolution = 10;


    //public Vector3 GetNodeSizeForRoad(RoadObject road, float nodeOffset)
    //{
    //    if (!HasIntersection()) return transform.position;

    //    Vector3 offset = Bezier.GetOffsettedPosition(transform.position, road.ControlPointPosition(), nodeOffset);
    //    return offset;
    //}


    //float GetMaxRoadWidth()
    //{
        //float roadWidth = 0;
        //foreach (RoadObject road in connectedRoads)
        //{
        //    float currentRoadWidth = road.RoadSegmentSO.roadWidth;

        //    if (currentRoadWidth > roadWidth)
        //        roadWidth = currentRoadWidth;
        //}
        //return roadWidth;
    //}

    public bool HasIntersection()
    {
        return connectedRoads.Count > 1;
    }

    public void AddRoadSegment(RoadObject segment)
    {
        if (!connectedRoads.Contains(segment))
        {
            connectedRoads.Add(segment);
            RefreshMeshEdgePoints();
        }
    }

    public void RefreshMeshEdgePoints()
    {
        //foreach (RoadObject segmentObject in connectedRoads)
        //{
        //    segmentObject.SetRoadMeshEdgePoints(this);
        //    segmentObject.UpdateRoad();
        //}
    }

    public void RemoveRoadSegment(RoadObject roadObject)
    {
        if (connectedRoads.Contains(roadObject))
            connectedRoads.Remove(roadObject);
    }

    public List<RoadObject> GetConnectedRoads()
    {
        return connectedRoads;
    }

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }


    //public List<RoadSegmentObject> GetAdjacentRoadsTo(RoadObject segment)
    //{
    //    List<RoadObject> adjacentRoads = connectedRoads.OrderBy(
    //        x => Vector3.Angle(segment.Direction(this), x.Direction(this)
    //    )).ToList<RoadObject>();

    //    //adjacentRoads.Remove(segment);

    //    return adjacentRoads;
    //}


    //Vector3[] GetNodePoints()
    //{
    //    Vector3 bezierPosition;
    //    int vertIndex = 0;
    //    float t;
    //    Vector3[] nodePoints = new Vector3[resolution * 4];

    //    foreach (RoadObject roadObject in connectedRoads)
    //    {
    //        List<RoadObject> adjacentRoads = GetAdjacentRoadsTo(roadObject);

    //        foreach (RoadObject adjacentRoad in adjacentRoads)
    //        {
    //            Vector3 roadLeftNodeMesh = roadObject.StartNode == this? roadObject.startLeftNodeMesh: roadObject.endLeftNodeMesh;
    //            Vector3 roadMidNodeMesh = roadObject.StartNode == this ? roadObject.startMidNodeMesh : roadObject.endMidNodeMesh;
    //            Vector3 roadRightNodeMesh = roadObject.StartNode == this ? roadObject.startRightNodeMesh : roadObject.endRightNodeMesh;

    //            Vector3 roadLeftRoadMesh = roadObject.StartNode == this ? roadObject.startLeftRoadMesh : roadObject.endLeftRoadMesh;
    //            Vector3 roadMidRoadMesh = roadObject.StartNode == this ? roadObject.startMidRoadMesh : roadObject.endMidRoadMesh;
    //            Vector3 roadRightRoadMesh = roadObject.StartNode == this ? roadObject.startRightRoadMesh : roadObject.endRightRoadMesh;

    //            Vector3 adjacentLeftNodeMesh = adjacentRoad.StartNode == this ? adjacentRoad.startLeftNodeMesh : adjacentRoad.endLeftNodeMesh;
    //            Vector3 adjacentMidNodeMesh = adjacentRoad.StartNode == this ? adjacentRoad.startMidNodeMesh : adjacentRoad.endMidNodeMesh;
    //            Vector3 adjacentRightNodeMesh = adjacentRoad.StartNode == this ? adjacentRoad.startRightNodeMesh : adjacentRoad.endRightNodeMesh;

    //            Vector3 adjacentLeftRoadMesh = adjacentRoad.StartNode == this ? adjacentRoad.startLeftRoadMesh : adjacentRoad.endLeftRoadMesh;
    //            Vector3 adjacentMidRoadMesh = adjacentRoad.StartNode == this ? adjacentRoad.startMidRoadMesh : adjacentRoad.endMidRoadMesh;
    //            Vector3 adjacentRightRoadMesh = adjacentRoad.StartNode == this ? adjacentRoad.startRightRoadMesh : adjacentRoad.endRightRoadMesh;

    //            Vector3 commonPoint = Position;
    //            Vector3 leftControlPoint = roadLeftNodeMesh;
    //            Vector3 righControlPoint = roadRightNodeMesh;

    //            if (adjacentRoads.Count == 3)
    //            {

    //            }
    //            else if (adjacentRoads.Count == 2)
    //            {
    //                //RafaelUtils.LineLineIntersection(
    //                //    out commonPoint,
    //                //    roadLeftNodeMesh,
    //                //    roadObject.Direction(this),
    //                //    adjacentLeftNodeMesh,
    //                //    adjacentRoad.Direction(this)
    //                //);
    //            }
    //            else if(adjacentRoads.Count == 1)
    //            {
    //                commonPoint = Position;
    //                leftControlPoint = roadLeftNodeMesh;
    //                righControlPoint = roadRightNodeMesh;
    //            }

    //            for (int i = 0; i < resolution; i++)
    //            {
    //                t = i / (float)(resolution - 1);
    //                bezierPosition = Bezier.QuadraticCurve(
    //                    roadLeftRoadMesh,
    //                    roadMidNodeMesh,
    //                    leftControlPoint,
    //                    t
    //                );
    //                nodePoints[vertIndex + 0] = transform.InverseTransformPoint(bezierPosition);
    //                nodePoints[vertIndex + 1] = transform.InverseTransformPoint(commonPoint);

    //                bezierPosition = Bezier.QuadraticCurve(
    //                    roadMidNodeMesh,
    //                    roadRightRoadMesh,
    //                    righControlPoint,
    //                    t
    //                );
    //                nodePoints[resolution * 2 + vertIndex + 0] = transform.InverseTransformPoint(bezierPosition);
    //                nodePoints[resolution * 2 + vertIndex + 1] = transform.InverseTransformPoint(commonPoint);

    //                vertIndex += 2;
    //            }

    //            int textureRepead = Mathf.RoundToInt(roadObject.RoadSegmentSO.tiling * Bezier.GetLengh(
    //                roadObject.StartNode.Position,
    //                roadObject.EndNode.Position
    //            ) * roadObject.RoadSegmentSO.spacing * .005f);
    //            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
    //            meshRenderer.material = roadObject.RoadMaterial;
    //            meshRenderer.material.mainTextureScale = new Vector2(.5f, 1);
    //            meshRenderer.material.mainTextureOffset = new Vector2(0, 0);
    //        }
    //    }


    //    //if (startNode.HasIntersection())
    //    //{
    //    //    Vector3 segmentDir = roadObject.Direction(startNode);
    //    //    segmentDir.Normalize();
    //    //    foreach (RoadSegmentObject adjacentSegment in startAdjacentRoads)
    //    //    {
    //    //        Vector3 secondRoadStartLeftRoadPoint = adjacentSegment.startLeftRoadMesh;
    //    //        Vector3 secondSegmentDir = adjacentSegment.Direction(startNode);

    //    //        RafaelUtils.LineLineIntersection(
    //    //            out leftIntersectionPoint,
    //    //            secondRoadStartLeftRoadPoint,
    //    //            secondSegmentDir,
    //    //            startLeftRoadMesh,
    //    //            segmentDir
    //    //        );
    //    //        startMidNodeMesh = leftIntersectionPoint;

    //    //    }

    //    //}

    //    return nodePoints;
    //}


}
