using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rafael.utils;


public class MeshUtilities {

    private readonly int resolution;

    private Vector3 startLeft;
    private Vector3 endLeft;
    private Vector3 controlLeft;

    private Vector3 startCenter;
    private Vector3 endCenter;
    private Vector3 controlCenter;

    private Vector3 startRight;
    private Vector3 endRight;
    private Vector3 controlRight;

    public MeshUtilities(
        int resolution,
        Vector3 startLeft,
        Vector3 endLeft,
        Vector3 controlLeft,
        Vector3 startCenter,
        Vector3 endCenter,
        Vector3 controlCenter,
        Vector3 startRight,
        Vector3 endRight,
        Vector3 controlRight) {
        this.resolution = resolution *= 3;

        this.startLeft = startLeft;
        this.endLeft = endLeft;
        this.controlLeft = controlLeft;

        this.startCenter = startCenter;
        this.endCenter = endCenter;
        this.controlCenter = controlCenter;

        this.startRight = startRight;
        this.endRight = endRight;
        this.controlRight = controlRight;
    }

    #region NodesWithoutIntersection
    /// <summary>
    /// Builds a mesh that has the same point for 
    /// both sides of the mesh AKA a Node without
    /// intersection
    /// </summary>
    /// <param name="meshData"></param>
    public MeshData PopulateStartNodeVerticesWOIntersection(MeshData meshData) {
        float t;

        for (int i = 0; i < resolution; i++) {
            t = i / (float)(resolution - 1);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(startCenter);
            meshData.AddVertice(rightRoadVertice);
        }
        return meshData;
    }

    /// <summary>
    /// Builds a mesh that ends at a common Point for
    /// both sides of the mesh AKA a Node without intersection
    /// </summary>
    /// <param name="meshData"></param>
    public MeshData PopulateEndNodeVerticesWOIntersection(MeshData meshData) {
        float t;
        for (int i = 0; i < resolution; i++) {
            t = i / (float)(resolution - 1);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(endCenter);
            meshData.AddVertice(rightRoadVertice);
        }
        return meshData;
    }
    #endregion

    #region NodesWithSingleIntersection
    /// <summary>
    /// Populates mesh for a starting node where the left side of a road meets the right
    /// side of the other
    /// </summary>
    /// <param name="meshData"></param>
    /// <returns></returns>
    public MeshData PopulateStartNodeMeshVerticesWSIntersection(MeshData meshData) {
        float t;

        for (int i = resolution / 2 - 1; i < resolution - 1; i++) {
            t = i / (float)(resolution - 3);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.QuadraticCurve(startCenter, endCenter, controlCenter, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
        return meshData;
    }

    /// <summary>
    /// Populates mesh for a starting node where the left side of a road meets the right
    /// side of the other
    /// </summary>
    /// <param name="meshData"></param>
    /// <returns></returns>
    public MeshData PopulateEndtNodeMeshVerticesWSIntersection(MeshData meshData) {
        float t;
        for (int i = 0; i < resolution / 2; i++) {
            t = i / (float)(resolution - 3);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.QuadraticCurve(startCenter, endCenter, controlCenter, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
        return meshData;
    }
    #endregion

    #region NodesWithDoubleIntersection
    /// <summary>
    /// Calculate the vertices for starting Node
    /// when the intersection has three or more 
    /// roads.
    /// It will start calculating half way into 
    /// the node and end at the begginig of the
    /// Road mesh
    /// </summary>
    /// <param name="meshData"></param>
    /// <returns></returns>
    public MeshData PopulateStartNodeMeshVerticesWDIntersection(MeshData meshData) {
        float t;
        for (int i = resolution / 2 - 1; i < resolution - 1; i++) {
            t = i / (float)(resolution - 3);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.LinearCurve(startCenter, endCenter, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
        return meshData;
    }

    /// <summary>
    /// Calculate the vertices for end Node
    /// when the intersection has three or more 
    /// roads.
    /// It will start calculating at the begginig 
    /// of the Road mesh and finish half way into
    /// the Node
    /// </summary>
    /// <param name="meshData"></param>
    /// <returns></returns>
    public MeshData PopulateEndNodeMeshVerticesWDIntersection(MeshData meshData) {
        float t;

        for (int i = 0; i < resolution / 2; i++) {
            t = i / (float)(resolution - 3);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.LinearCurve(startCenter, endCenter, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
        return meshData;
    }
    #endregion

    public MeshData PopulateRoadMeshVertices(MeshData meshData) {
        float t;

        for (int i = 0; i < resolution; i++) {
            t = i / (float)(resolution - 1);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endRight, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.QuadraticCurve(startCenter, endCenter, controlCenter, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endLeft, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
        return meshData;
    }

    /// <summary>
    /// Calculate the mesh's UVs for given vertices
    /// </summary>
    /// <param name="meshData"></param>
    internal static void PopulateMeshUvs(MeshData meshData) {
        Vector2[] uvs = new Vector2[3];
        int numUvs = meshData.vertices.Count / 3;
        for (int i = 0; i < numUvs; i++) {
            float completionPercent = i / (float)(numUvs - 1);

            uvs[0] = new Vector2(0, completionPercent);
            uvs[1] = new Vector2(1, completionPercent);
            uvs[2] = new Vector2(0, completionPercent);

            meshData.AddUvs(uvs);
        }
    }

    /// <summary>
    /// Calculate the mesh triangles for given vertices
    /// </summary>
    /// <param name="meshData"></param>
    internal static void PopulateMeshTriangles(MeshData meshData) {
        int[] triangles = new int[12];
        int vertIndex = 0;
        int numTriangles = meshData.vertices.Count / 3;
        for (int i = 0; i < numTriangles - 1; i++) {
            triangles[0] = vertIndex + 0;
            triangles[1] = vertIndex + 1;
            triangles[2] = vertIndex + 3;

            triangles[3] = vertIndex + 1;
            triangles[4] = vertIndex + 4;
            triangles[5] = vertIndex + 3;

            triangles[6] = vertIndex + 1;
            triangles[7] = vertIndex + 2;
            triangles[8] = vertIndex + 4;

            triangles[9] = vertIndex + 2;
            triangles[10] = vertIndex + 5;
            triangles[11] = vertIndex + 4;

            vertIndex += 3;
            meshData.AddTriangles(triangles);
        }
    }

    /// <summary>
    /// Takes in a road and its node position as well as
    /// the adjacent road and the node connecting both roads
    /// and returns the adjacent road node offsetted and the 
    /// start of the mesh for the road
    /// </summary>
    /// <param name="adjecentRoad"></param>
    /// <param name="node"></param>
    /// <param name="roadPosition"></param>
    /// <param name="nodePosition"></param>
    /// <param name="adjacentRoadNodeMeshPosition"></param>
    /// <param name="adjacentRoadControlNodePosition"></param>
    public static void GetNodeMeshPositions(
        RoadObject adjecentRoad,
        Node node,
        Vector3 roadPosition,
        Vector3 nodePosition,
        out Vector3 adjacentRoadNodeMeshPosition,
        out Vector3 adjacentRoadControlNodePosition) {

        Node otherNode = adjecentRoad.OtherNodeTo(node);
        float offsetDistance = node.GetNodeSizeForRoad(adjecentRoad);
        Vector3 otherNodePostion = otherNode.Position - roadPosition;
        adjacentRoadControlNodePosition = adjecentRoad.ControlNodeObject.transform.position - roadPosition;

        adjacentRoadNodeMeshPosition = Bezier.GetOffsettedPosition(nodePosition, otherNodePostion, adjacentRoadControlNodePosition, offsetDistance);
    }

}
