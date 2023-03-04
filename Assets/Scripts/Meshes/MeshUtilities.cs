using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class MeshUtilities {


    public static void PopulateStartNodeWOIntersection(MeshData meshData, int roadWidth, Vector3 startPosition, Vector3 controlPosition, int resolution) {
        float t;
        resolution *= 3;
        // Assumes no intersection, start for left and right are the same
        Vector3 startNodeMeshPosition = startPosition + (startPosition - controlPosition).normalized * roadWidth / 2;

        Vector3 endNodeMeshLeftPosition = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startPosition, controlPosition);
        Vector3 endNodeMeshRightPosition = RoadUtilities.GetRoadRightSideVertice(roadWidth, startPosition, controlPosition);

        Vector3 left = (endNodeMeshLeftPosition - startPosition).normalized;
        Vector3 leftControlNodePostion = startNodeMeshPosition + left * roadWidth / 2;
        Vector3 rightControlNodePosition = startNodeMeshPosition - left * roadWidth / 2;

        for (int i = 0; i < resolution; i++) {
            t = i / (float)(resolution - 1);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startNodeMeshPosition, endNodeMeshLeftPosition, leftControlNodePostion, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startNodeMeshPosition, endNodeMeshRightPosition, rightControlNodePosition, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(startPosition);
            meshData.AddVertice(rightRoadVertice);
        }
    }

    public static void PopulateEndtNodeWOIntersection(MeshData meshData, int roadWidth, Vector3 endPosition, Vector3 controlPosition, int resolution) {
        float t;
        resolution *= 3;
        // Assumes no intersection, start for left and right are the same
        Vector3 startNodeMeshPosition = endPosition + (endPosition - controlPosition).normalized * roadWidth / 2;

        Vector3 endNodeMeshLeftPosition = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endPosition, controlPosition);
        Vector3 endNodeMeshRightPosition = RoadUtilities.GetRoadRightSideVertice(roadWidth, endPosition, controlPosition);

        Vector3 left = (endNodeMeshLeftPosition - endPosition).normalized;
        Vector3 leftControlNodePostion = startNodeMeshPosition + left * roadWidth / 2;
        Vector3 rightControlNodePosition = startNodeMeshPosition - left * roadWidth / 2;

        for (int i = 0; i < resolution; i++) {
            t = i / (float)(resolution - 1);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(endNodeMeshLeftPosition, startNodeMeshPosition, leftControlNodePostion, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(endNodeMeshRightPosition, startNodeMeshPosition, rightControlNodePosition, t);

            meshData.AddVertice(rightRoadVertice);
            meshData.AddVertice(endPosition);
            meshData.AddVertice(leftRoadVertice);
        }
    }

    public static void PopulateStartNodeWIntersection(MeshData meshData, RoadObject roadObject, Node startNode, Node endNode, int resolution) {

        int roadWidth = roadObject.GetRoadWidth();
        float thiRoadOffsetDistance = startNode.GetNodeSizeForRoad(roadObject);
        Vector3 roadPosition = roadObject.transform.position;
        Vector3 startNodePostion = startNode.Position;
        Vector3 endNodePosition = endNode.Position;
        Vector3 controlPosition = roadObject.ControlNodeObject.transform.position;

        Vector3 startPosition = Vector3.negativeInfinity;
        Vector3 endPosition;

        // If node has no intersection just run the normal function
        if (!startNode.HasIntersection()) {
            startNodePostion -= roadPosition;
            controlPosition -= roadPosition;
            PopulateStartNodeWOIntersection(meshData, roadWidth, startNodePostion, controlPosition, resolution);
            return;
        }
        Dictionary<RoadObject, float> adjacentRoads = startNode.GetAdjacentRoadsTo(roadObject);
        endPosition = Bezier.GetOffsettedPosition(startNodePostion, endNodePosition, controlPosition, thiRoadOffsetDistance);


        if (adjacentRoads.Count == 1) {
            foreach (RoadObject adjecentRoad in adjacentRoads.Keys) {
                Node otherNode = adjecentRoad.OtherNodeTo(startNode);
                float otherRoadOffsetDistance = startNode.GetNodeSizeForRoad(adjecentRoad);
                Vector3 adjacentRoadPosition = adjecentRoad.transform.position;
                Vector3 otherNodeEndPostion = otherNode.Position;
                Vector3 otherNodeControlPostion = adjecentRoad.ControlNodeObject.transform.position;

                startPosition = Bezier.GetOffsettedPosition(startNodePostion, otherNodeEndPostion, otherNodeControlPostion, otherRoadOffsetDistance);

                endPosition -= roadPosition;
                startPosition -= roadPosition;
                startNodePostion -= roadPosition;

                PopulateStartNodeWSingleIntersection(meshData, startPosition, endPosition, startNodePostion, roadWidth, resolution);
                return;
            }
        }
        Vector3 startLeftPosition = Vector3.negativeInfinity;
        Vector3 startRightPosition = Vector3.negativeInfinity;

        Vector3 leftRoadNodeControlPostion = Vector3.negativeInfinity;
        Vector3 rightRoadNodeControlPosition = Vector3.negativeInfinity;

        foreach (RoadObject adjecentRoad in adjacentRoads.Keys) {
            if (adjacentRoads.GetValueOrDefault(adjecentRoad) > 0) {
                // road is to the left
                Node leftRoadOtherNode = adjecentRoad.OtherNodeTo(startNode);
                float leftRoadOffsetDistance = startNode.GetNodeSizeForRoad(adjecentRoad);
                Vector3 leftAdjacentRoadPosition = adjecentRoad.transform.position;
                Vector3 leftRoadOtherNodePostion = leftRoadOtherNode.Position;
                leftRoadNodeControlPostion = adjecentRoad.ControlNodeObject.transform.position;

                startLeftPosition = Bezier.GetOffsettedPosition(startNodePostion, leftRoadOtherNodePostion, leftRoadNodeControlPostion, leftRoadOffsetDistance);
            } else {
                // road is to the right
                Node rightRoadOtherNode = adjecentRoad.OtherNodeTo(startNode);
                float rightRoadOffsetDistance = startNode.GetNodeSizeForRoad(adjecentRoad);
                Vector3 rightRoadOtherNodePosition = rightRoadOtherNode.Position;
                rightRoadNodeControlPosition = adjecentRoad.ControlNodeObject.transform.position;

                startRightPosition = Bezier.GetOffsettedPosition(startNodePostion, rightRoadOtherNodePosition, rightRoadNodeControlPosition, rightRoadOffsetDistance);
            }

        }

        if (startLeftPosition == Vector3.negativeInfinity
            || startRightPosition == Vector3.negativeInfinity
            || leftRoadNodeControlPostion == Vector3.negativeInfinity
            || rightRoadNodeControlPosition == Vector3.negativeInfinity)
            Debug.LogError("Shit!");



        Vector3 leftRoadRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startLeftPosition, leftRoadNodeControlPostion);
        Vector3 thisRoadRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, endPosition, controlPosition);
        Vector3 controlLeft;

        Vector3 rightRoadLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startRightPosition, rightRoadNodeControlPosition);
        Vector3 thisRoadLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endPosition, controlPosition);
        Vector3 controlRight;

        Vector3 n0Left = (leftRoadRight - startLeftPosition).normalized;
        Vector3 n1Left = (thisRoadLeft - endPosition).normalized;

        Vector3 n0Right = (rightRoadLeft - startRightPosition).normalized;
        Vector3 n1Right = (thisRoadRight - endPosition).normalized;



        if (Vector3.Angle(n0Left, n1Left) != 0) {
            // Road is NOT straight, so the DOT product is not 0!
            // This fails for angles > 90, so we must deal with it later
            controlLeft = startNodePostion + ((n0Left + n1Left) * roadWidth) / Vector3.Dot((n0Left + n1Left), (n0Left + n1Left));
        } else {
            // Road is traight, so calculations are easier
            controlLeft = startNodePostion + n0Left * roadWidth / 2;
        }

        if (Vector3.Angle(n0Right, n1Right) != 0) {
            // Road is NOT straight, so the DOT product is not 0!
            // This fails for angles > 90, so we must deal with it later
            controlRight = startNodePostion + ((n0Right + n1Right) * roadWidth) / Vector3.Dot((n0Right + n1Right), (n0Right + n1Right));
        } else {
            // Road is traight, so calculations are easier
            controlRight = startNodePostion + n1Right * roadWidth / 2;
        }

        leftRoadRight -= roadPosition;
        thisRoadLeft -= roadPosition;

        endPosition -= roadPosition;
        startNodePostion -= roadPosition;

        rightRoadLeft -= roadPosition;
        thisRoadRight -= roadPosition;

        controlRight -= roadPosition;
        controlLeft -= roadPosition;

        Vector3 startCenterNode = startNodePostion + (startNodePostion - endPosition);

        resolution *= 3;
        float t;
        for (int i = resolution / 2 - 1; i < resolution - 1; i++) {
            t = i / (float)(resolution - 2);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(leftRoadRight, thisRoadLeft, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.LinearCurve(startCenterNode, endPosition, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(rightRoadLeft, thisRoadRight, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
    }



    public static void PopulateEndNodeWIntersections(MeshData meshData, RoadObject roadObject, Node startNode, Node endNode, int resolution) {

        int roadWidth = roadObject.GetRoadWidth();
        float thiRoadOffsetDistance = endNode.GetNodeSizeForRoad(roadObject);
        Vector3 roadPosition = roadObject.transform.position;
        Vector3 startNodePosition = startNode.Position;
        Vector3 endNodePosition = endNode.Position;
        Vector3 controlPosition = roadObject.ControlNodeObject.transform.position;

        Vector3 startPosition = Vector3.negativeInfinity;
        Vector3 endPosition = Vector3.negativeInfinity;

        // If node has no intersection just run the normal function
        if (!endNode.HasIntersection()) {
            endNodePosition -= roadPosition;
            controlPosition -= roadPosition;
            PopulateStartNodeWOIntersection(meshData, roadWidth, endNodePosition, controlPosition, resolution);
            return;
        }
        Dictionary<RoadObject, float> adjacentRoads = endNode.GetAdjacentRoadsTo(roadObject);
        startPosition = Bezier.GetOffsettedPosition(endNodePosition, startNodePosition, controlPosition, thiRoadOffsetDistance);


        if (adjacentRoads.Count == 1) {
            foreach (RoadObject adjecentRoad in adjacentRoads.Keys) {
                Node otherNode = adjecentRoad.OtherNodeTo(endNode);
                float otherRoadOffsetDistance = endNode.GetNodeSizeForRoad(adjecentRoad);
                Vector3 adjacentRoadPosition = adjecentRoad.transform.position;
                Vector3 otherNodeEndPostion = otherNode.Position;
                Vector3 otherNodeControlPostion = adjecentRoad.ControlNodeObject.transform.position;

                endPosition = Bezier.GetOffsettedPosition(endNodePosition, otherNodeEndPostion, otherNodeControlPostion, otherRoadOffsetDistance);

                endPosition -= roadPosition;
                startPosition -= roadPosition;
                endNodePosition -= roadPosition;

                PopulateEndNodeWSingleIntersection(meshData, startPosition, endPosition, endNodePosition, roadWidth, resolution);
                return;
            }
        }
        
        Vector3 endLeftPosition = Vector3.negativeInfinity;
        Vector3 endRightPosition = Vector3.negativeInfinity;

        Vector3 leftRoadNodeControlPostion = Vector3.negativeInfinity;
        Vector3 rightRoadNodeControlPosition = Vector3.negativeInfinity;

        foreach (RoadObject adjecentRoad in adjacentRoads.Keys) {
            if (adjacentRoads.GetValueOrDefault(adjecentRoad) > 0) {
                // road is to the left
                Node leftRoadOtherNode = adjecentRoad.OtherNodeTo(endNode);
                float leftRoadOffsetDistance = endNode.GetNodeSizeForRoad(adjecentRoad);
                Vector3 leftRoadOtherNodePostion = leftRoadOtherNode.Position;
                leftRoadNodeControlPostion = adjecentRoad.ControlNodeObject.transform.position;

                endLeftPosition = Bezier.GetOffsettedPosition(endNodePosition, leftRoadOtherNodePostion, leftRoadNodeControlPostion, leftRoadOffsetDistance);
            } else {
                // road is to the right
                Node rightRoadOtherNode = adjecentRoad.OtherNodeTo(endNode);
                float rightRoadOffsetDistance = endNode.GetNodeSizeForRoad(adjecentRoad);
                Vector3 rightRoadOtherNodePosition = rightRoadOtherNode.Position;
                rightRoadNodeControlPosition = adjecentRoad.ControlNodeObject.transform.position;

                endRightPosition = Bezier.GetOffsettedPosition(endNodePosition, rightRoadOtherNodePosition, rightRoadNodeControlPosition, rightRoadOffsetDistance);
            }

        }

        if (endLeftPosition == Vector3.negativeInfinity
            || endRightPosition == Vector3.negativeInfinity
            || leftRoadNodeControlPostion == Vector3.negativeInfinity
            || rightRoadNodeControlPosition == Vector3.negativeInfinity)
            Debug.LogError("Shit!");



        Vector3 leftRoadRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, endLeftPosition, leftRoadNodeControlPostion);
        Vector3 thisRoadRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startPosition, controlPosition);
        Vector3 controlLeft;

        Vector3 rightRoadLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endRightPosition, rightRoadNodeControlPosition);
        Vector3 thisRoadLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startPosition, controlPosition);
        Vector3 controlRight;

        Vector3 n0Left = (leftRoadRight - endLeftPosition).normalized;
        Vector3 n1Left = (thisRoadLeft - startPosition).normalized;

        Vector3 n0Right = (rightRoadLeft - endRightPosition).normalized;
        Vector3 n1Right = (thisRoadRight - startPosition).normalized;



        if (Vector3.Angle(n0Left, n1Left) != 0) {
            // Road is NOT straight, so the DOT product is not 0!
            // This fails for angles > 90, so we must deal with it later
            controlLeft = endNodePosition + ((n0Left + n1Left) * roadWidth) / Vector3.Dot((n0Left + n1Left), (n0Left + n1Left));
        } else {
            // Road is traight, so calculations are easier
            controlLeft = endNodePosition + n0Left * roadWidth / 2;
        }

        if (Vector3.Angle(n0Right, n1Right) != 0) {
            // Road is NOT straight, so the DOT product is not 0!
            // This fails for angles > 90, so we must deal with it later
            controlRight = endNodePosition + ((n0Right + n1Right) * roadWidth) / Vector3.Dot((n0Right + n1Right), (n0Right + n1Right));
        } else {
            // Road is traight, so calculations are easier
            controlRight = endNodePosition + n1Right * roadWidth / 2;
        }

        leftRoadRight -= roadPosition;
        thisRoadLeft -= roadPosition;

        startPosition -= roadPosition;
        endNodePosition -= roadPosition;

        rightRoadLeft -= roadPosition;
        thisRoadRight -= roadPosition;

        controlRight -= roadPosition;
        controlLeft -= roadPosition;

        Vector3 startCenterNode = endNodePosition + (endNodePosition - startPosition);

        resolution *= 3;
        float t;
        for (int i = 0; i < resolution / 2; i++) {
            t = i / (float)(resolution - 2);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(thisRoadLeft, leftRoadRight, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.LinearCurve(startPosition, startCenterNode, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(thisRoadRight, rightRoadLeft, controlRight, t);

            meshData.AddVertice(rightRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(leftRoadVertice);
        }
    }


    private static void PopulateStartNodeWSingleIntersection(MeshData meshData, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int roadWidth, int resolution) {
        resolution *= 3;
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
            controlLeft = controlPosition + ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
            controlRight = controlPosition - ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
        } else {
            // Road is traight, so calculations are easier
            controlLeft = controlPosition + n0 * roadWidth / 2;
            controlRight = controlPosition - n1 * roadWidth / 2;
        }

        for (int i = resolution / 2 - 1; i < resolution; i++) {
            t = i / (float)(resolution - 2);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endRight, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.QuadraticCurve(startPosition, endPosition, controlPosition, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endLeft, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
    }

    private static void PopulateEndNodeWSingleIntersection(MeshData meshData, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int roadWidth, int resolution) {
        resolution *= 3;
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
            controlLeft = controlPosition + ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
            controlRight = controlPosition - ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
        } else {
            // Road is traight, so calculations are easier
            controlLeft = controlPosition + n0 * roadWidth / 2;
            controlRight = controlPosition - n1 * roadWidth / 2;
        }

        for (int i = 0; i < resolution / 2; i++) {
            t = i / (float)(resolution - 2);
            Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endRight, controlLeft, t);
            Vector3 centerRoadVertice = Bezier.QuadraticCurve(startPosition, endPosition, controlPosition, t);
            Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endLeft, controlRight, t);

            meshData.AddVertice(leftRoadVertice);
            meshData.AddVertice(centerRoadVertice);
            meshData.AddVertice(rightRoadVertice);
        }
    }

    public static void PopulateRoadMeshVertices(MeshData meshData, RoadObject roadObject, int resolution = 10) {
        Node startNode = roadObject.StartNode;
        Node endNode = roadObject.EndNode;
        
        Vector3 roadPosition = roadObject.transform.position;
        Vector3 startPosition = startNode.Position;
        Vector3 endPosition = endNode.Position;
        Vector3 controlPosition = roadObject.ControlNodeObject.transform.position;

        int roadWidth = roadObject.GetRoadWidth();
        float offsetDistance;

        if (startNode.HasIntersection()) {
            offsetDistance = startNode.GetNodeSizeForRoad(roadObject);
            startPosition = Bezier.GetOffsettedPosition(startPosition, endPosition, controlPosition, offsetDistance);
        }
        if (endNode.HasIntersection()) {
            offsetDistance = endNode.GetNodeSizeForRoad(roadObject);
            endPosition = Bezier.GetOffsettedPosition(endPosition, startPosition, controlPosition, offsetDistance);
        }

        startPosition -= roadPosition;
        endPosition -= roadPosition;
        controlPosition -= roadPosition;
        PopulateRoadMeshVertices(meshData, roadWidth, startPosition, endPosition, controlPosition, resolution);
    }

    public static void PopulateRoadMeshVertices(MeshData meshData, int roadWidth, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int resolution) {

        resolution *= 3;
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

    private static GameObject CreateSphere(Vector3 position, string name, float scale = .25f) {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = scale * Vector3.one;
        sphere.transform.position = position;
        sphere.transform.name = name;
        return sphere;
    }
}
