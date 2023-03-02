using System;
using System.Collections;
using UnityEngine;


public static class MeshUtilities {


    public static void PopulateNodeMeshVertices(MeshData meshData, float roadWidth, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int resolution = 20) {
        float t;
        resolution *= 3;
        // Assumes no intersection, start for left and right are the same
        Vector3 startNodeMeshPosition = startPosition + (startPosition - controlPosition).normalized * roadWidth / 2;

        Vector3 endNodeMeshLeftPosition = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startPosition, controlPosition);
        Vector3 endNodeMeshRightPosition = RoadUtilities.GetRoadRightSideVertice(roadWidth, startPosition, controlPosition);

        Vector3 left = (endNodeMeshLeftPosition - startPosition).normalized;
        Vector3 leftControlNodePostion = startNodeMeshPosition + left * roadWidth / 2;
        Vector3 rightControlNodePosition = startNodeMeshPosition - left * roadWidth / 2;

        Debug.DrawLine(endNodeMeshLeftPosition, leftControlNodePostion, Color.red);
        Debug.DrawLine(leftControlNodePostion, startNodeMeshPosition, Color.red);
        Debug.DrawLine(startNodeMeshPosition, rightControlNodePosition, Color.red);
        Debug.DrawLine(rightControlNodePosition, endNodeMeshRightPosition, Color.red);

        for (int i = 0; i < resolution; i++) {
            t = i / (float)(resolution - 1);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startNodeMeshPosition, endNodeMeshLeftPosition, leftControlNodePostion, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startNodeMeshPosition, endNodeMeshRightPosition, rightControlNodePosition, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(startPosition);
            meshData.AddVertice(rightRoadVertice);
        }
    }

    public static void PopulateNodeMeshVertices(MeshData meshData, RoadObject roadObject, Node startNode, Node endNode, int resolution = 10) {

        float roadWidth = roadObject.GetRoadWidth();
        Vector3 nodePosition = startNode.Position;
        Vector3 controlPosition = roadObject.ControlNodeObject.transform.position;
        Vector3 roadPosition = roadObject.transform.position;

        // If node has no intersection just run the normal function
        if (!startNode.HasIntersection()) {
            nodePosition = roadPosition - startNode.Position;
            controlPosition = roadObject.ControlNodeObject.transform.position - roadPosition;
            PopulateNodeMeshVertices(meshData, roadWidth, nodePosition, endNode.transform.position, controlPosition, resolution) ;
            return;
        }
    }

    public static void PopulateRoadMeshVertices(MeshData meshData, RoadObject roadObject, int resolution = 10) {
        Node startNode = roadObject.StartNode;
        Node endNode = roadObject.EndNode;
        
        Vector3 roadPosition = roadObject.transform.position;
        Vector3 startPosition = startNode.Position - roadPosition;
        Vector3 endPosition = endNode.Position - roadPosition;
        Vector3 controlPosition = roadObject.ControlNodeObject.transform.position - roadPosition;

        float offsetDistance = roadObject.GetRoadWidth();
        float roadWidth = roadObject.GetRoadWidth();

        if (startNode.HasIntersection()) {
            startPosition = Bezier.GetOffsettedPosition(startPosition, endPosition, controlPosition, offsetDistance);
        }
        if (endNode.HasIntersection()) {
            endPosition = Bezier.GetOffsettedPosition(endPosition, startPosition, controlPosition, offsetDistance);
        }

        PopulateRoadMeshVertices(meshData, roadWidth, startPosition, endPosition, controlPosition, resolution);
    }


    public static void PopulateRoadMeshVertices(MeshData meshData, float roadWidth, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int resolution = 20) {

        resolution = resolution * 3;
        float t;
        Vector3 startLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startPosition, controlPosition);
        Vector3 endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endPosition, controlPosition);
        Vector3 controlLeft;

        Vector3 startRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startPosition, controlPosition);
        Vector3 endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, endPosition, controlPosition);
        Vector3 controlRight;

        Vector3 n0 = (startLeft - startPosition).normalized;
        Vector3 n1 = (endRight - endPosition).normalized;

        if (Vector3.Angle(n0, n1) != 0) {
            // Road is NOT straight, so the DOT product is not 0!
            // This fails for angles > 90, so we must deal with it later
            controlLeft = controlPosition + ((n0 + n1) * roadWidth)/Vector3.Dot((n0 + n1), (n0 + n1));
            controlRight = controlPosition - ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
        } else {
            // Road is traight, so calculations are easier
            controlLeft = controlPosition + n0 * roadWidth / 2;
            controlRight = controlPosition - n1 * roadWidth / 2;
        }

        for (int i = 0; i < resolution; i++) {
            t = i / (float)(resolution - 1);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endRight, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.QuadraticCurve(startPosition, endPosition, controlPosition, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endLeft, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
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

}