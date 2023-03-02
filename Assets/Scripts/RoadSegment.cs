using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using rafael.utils;

[CreateAssetMenu]
public class RoadSegment: ScriptableObject
{

    [SerializeField, Range(2, 20)]
    int resolution;

    public GameObject roadObject;
    public GameObject nodeGFX;

    public float roadWidth = 1;
    [Range(.05f, 1.5f)]
    public float spacing = 1;
    public Material roadMaterial;
    public float tiling = 1;

    public Vector3 leftIntersectionPoint;
    public Vector3 rightIntersectionPoint;

    public GameObject CreateRoadSegment(GameObject node_1, GameObject node_2, GameObject controlPoint)
    {
        Vector3[] pointsPositions = new Vector3[3];
        pointsPositions[0] = node_1.transform.position;
        pointsPositions[1] = node_2.transform.position;
        pointsPositions[2] = controlPoint.transform.position;

        GameObject roadSegmentModel = Instantiate(roadObject);
        roadSegmentModel.GetComponent<RoadSegmentObject>().RoadSegmentSO = this;
        return roadObject;
    }

    public RoadSegmentObject CreateRoadSegment(GameObject node_1, GameObject node_2)
    {
        Vector3 startPos = node_1.transform.position;
        Vector3 endPos = node_2.transform.position;

        Vector3 controlNodePos = (startPos + endPos) / 2;
        GameObject roadSegmentModel = Instantiate(roadObject);
        RoadSegmentObject roadSegmentObject = roadSegmentModel.GetComponent<RoadSegmentObject>();
        roadSegmentObject.Init(
            node_1,
            node_2, 
            controlNodePos, 
            this, 
            roadMaterial
        );

        return roadSegmentObject;
    }


    Mesh GetRoadMesh(Vector3[] points)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * (points.Length - 1);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < points.Length - 1)
                forward += points[i + 1] - points[i];
            if (i > 0)
                forward += points[i] - points[i - 1];

            forward.Normalize();
            Vector3 left = new Vector3(-forward.z, forward.y, forward.x);
            verts[vertIndex] = points[i] + .5f * roadWidth * left;
            verts[vertIndex + 1] = points[i] - .5f * roadWidth * left;

            float completionPercent = i / (float)(points.Length - 1);
            uvs[vertIndex] = new Vector2(0, completionPercent);
            uvs[vertIndex + 1] = new Vector2(1, completionPercent);

            if (i < points.Length - 1)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = vertIndex + 2;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = vertIndex + 2;
                tris[triIndex + 5] = vertIndex + 3;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh
        {
            vertices = verts,
            triangles = tris,
            uv = uvs
        };
        return mesh;
    }

    Vector3[] GetRoadPoints(RoadSegmentObject roadObject)
    {
        Vector3 bezierPosition;
        int vertIndex = 0;
        float t;
        Vector3[] roadPoints = new Vector3[resolution * 3];

        Node startNode = roadObject.StartNode;
        Node endNode = roadObject.EndNode;

        roadObject.SetRoadMeshEdgePoints(startNode);
        roadObject.SetRoadMeshEdgePoints(endNode);

        Vector3 startMidRoadMesh = roadObject.startMidRoadMesh;
        Vector3 startLeftRoadMesh = roadObject.startLeftRoadMesh;
        Vector3 startRightRoadMesh = roadObject.startRightRoadMesh;

        Vector3 endMidRoadMesh = roadObject.endMidRoadMesh;
        Vector3 endLeftRoadMesh = roadObject.endLeftRoadMesh;
        Vector3 endRightRoadMesh = roadObject.endRightRoadMesh;

        for (int i = 0; i < resolution; i++)
        {
            t = i / (float)(resolution - 1);
            bezierPosition = Bezier.LinearCurve(startLeftRoadMesh, endRightRoadMesh, t);
            roadPoints[vertIndex + 0] = bezierPosition;

            bezierPosition = Bezier.LinearCurve(startMidRoadMesh, endMidRoadMesh, t);
            roadPoints[vertIndex + 1] = bezierPosition;

            bezierPosition = Bezier.LinearCurve(startRightRoadMesh, endLeftRoadMesh, t);
            roadPoints[vertIndex + 2] = bezierPosition;

            vertIndex += 3;

        }
        
        return roadPoints;
    }

    public Mesh CreateRoadMesh(RoadSegmentObject roadObject)
    {
        Vector3[] verts = GetRoadPoints(roadObject);
        Vector2[] uvs = new Vector2[verts.Length];

        int numTris = 2 * (verts.Length - 1);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < verts.Length - 1; i++)
        {
            Debug.DrawLine(verts[i], verts[i + 1], Color.red);
        }

        for (int i = 0; i < verts.Length / 3 - 1; i++)
        {
            tris[triIndex + 0] = vertIndex + 0;
            tris[triIndex + 1] = vertIndex + 1;
            tris[triIndex + 2] = vertIndex + 3;
                          
            tris[triIndex + 3] = vertIndex + 1;
            tris[triIndex + 4] = vertIndex + 4;
            tris[triIndex + 5] = vertIndex + 3;
                          
            tris[triIndex + tris.Length / 2 + 0] = vertIndex + 2;
            tris[triIndex + tris.Length / 2 + 1] = vertIndex + 4;
            tris[triIndex + tris.Length / 2 + 2] = vertIndex + 1;

            tris[triIndex + tris.Length / 2 + 3] = vertIndex + 2;
            tris[triIndex + tris.Length / 2 + 4] = vertIndex + 5;
            tris[triIndex + tris.Length / 2 + 5] = vertIndex + 4;

            triIndex += 6;
            vertIndex += 3;

        }
        vertIndex = 0;
        for (int i = 0; i < verts.Length / 3; i++)
        {
            float completionPercent = i / (float)(verts.Length / 3 - 1);
            uvs[vertIndex + 0] = new Vector2(0, completionPercent);
            uvs[vertIndex + 1] = new Vector2(1, completionPercent);
            uvs[vertIndex + 2] = new Vector2(0, completionPercent);
            vertIndex += 3;
        }

        Mesh mesh = new Mesh
        {
            vertices = verts,
            triangles = tris,
            uv = uvs
        };

        return mesh;
    }


    public Mesh CreateTemporaryRoadMesh(Vector3 startPos, Vector3 endPos)
    {
        Vector3[] points = new Vector3[resolution];
        Vector3 bezierPosition;
        float t;
        for (int i = 0; i < resolution; i++)
        {
            t = i / (float)(resolution - 1);
            bezierPosition = Bezier.LinearCurve(startPos, endPos, t);
            points[i] = bezierPosition;
        }

        Mesh mesh = GetRoadMesh(points);

        return mesh;
    }
}
