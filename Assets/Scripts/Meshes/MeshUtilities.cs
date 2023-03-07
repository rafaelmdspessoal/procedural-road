using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class MeshUtilities {

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

    public static MeshData PopulateMeshVertices(
        MeshData meshData, 
        int resolution, 
        Vector3 startLeft, 
        Vector3 endLeft,
        Vector3 controlLeft,
        Vector3 startCenter,
        Vector3 endCenterNode,
        Vector3 startRight,
        Vector3 endRight,
        Vector3 controlRight) {
        resolution *= 3;
        float t;
        for (int i = resolution / 2 - 1; i < resolution - 1; i++) {
            t = i / (float)(resolution - 2);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.LinearCurve(startCenter, endCenterNode, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
        return meshData;
    }

    public static MeshData PopulateEndNodeMeshVertices(
        MeshData meshData,
        int resolution,
        Vector3 startLeft,
        Vector3 endLeft,
        Vector3 controlLeft,
        Vector3 startCenter,
        Vector3 endCenterNode,
        Vector3 startRight,
        Vector3 endRight,
        Vector3 controlRight) {
        resolution *= 3;
        float t;

        for (int i = 0; i < resolution / 2; i++) {
            t = i / (float)(resolution - 2);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.LinearCurve(startCenter, endCenterNode, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
        return meshData;
    }

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

    public static void PopulateMeshTriangles(MeshData meshData) {
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

    public static GameObject CreateSphere(Vector3 position, string name, float scale = .25f) {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = scale * Vector3.one;
        sphere.transform.position = position;
        sphere.transform.name = name;
        return sphere;
    }
}
