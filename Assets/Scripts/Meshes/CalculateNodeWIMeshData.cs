using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Road.Mesh.NodeVertices {
    public class CalculateNodeWIMeshData {

        private readonly RoadObject roadObject;
        private readonly Node startNode;
        private readonly Node endNode;

        private readonly int resolution;
        private readonly int roadWidth;

        private static float roadOffsetDistance;

        Vector3 roadPosition;
        Vector3 startNodePosition;
        Vector3 endNodePosition;
        Vector3 controlPosition;

        private static RoadObject adjecentRoad;

        private static Vector3 startCenterNode;
        private static Vector3 endCenterNode;

        private static Vector3 leftRoadCenter = Vector3.negativeInfinity;
        private static Vector3 rightRoadCenter = Vector3.negativeInfinity;

        private static Vector3 leftRoadControlNode = Vector3.negativeInfinity;
        private static Vector3 rightRoadControlNode = Vector3.negativeInfinity;

        private static Vector3 controlLeft;
        private static Vector3 controlRight;

        public CalculateNodeWIMeshData(RoadObject roadObject) {
            // Offet nodes by the road postion so we build the mesh in
            // world space, not local
            this.roadObject = roadObject;

            startNode = roadObject.StartNode;
            endNode = roadObject.EndNode;     
            roadPosition = roadObject.transform.position;
            startNodePosition = startNode.Position - roadPosition;
            endNodePosition = endNode.Position - roadPosition;
            controlPosition = roadObject.ControlNodeObject.transform.position - roadPosition;
            roadWidth = roadObject.RoadWidth;
            resolution = roadObject.RoadResolution;
        }

        public MeshData PopulateStartNode(MeshData meshData) {

            roadOffsetDistance = startNode.GetNodeSizeForRoad(roadObject);
            endCenterNode = Bezier.GetOffsettedPosition(startNodePosition, endNodePosition, controlPosition, roadOffsetDistance);
            startCenterNode = startNodePosition + (startNodePosition - endCenterNode);

            // If node has no intersection just run the normal function
            if (!startNode.HasIntersection()) {
                CalculateNodeWOIMeshData.PopulateStartNode(meshData, roadWidth, startNodePosition, controlPosition, resolution);
                return meshData;
            }

            Dictionary<float, RoadObject> adjacentRoads = startNode.GetAdjacentRoadsTo(roadObject);
            if (adjacentRoads.Count == 1) {
                foreach (float adjecentRoadAngle in adjacentRoads.Keys) {
                    adjecentRoad = adjacentRoads.GetValueOrDefault(adjecentRoadAngle);
                    MeshUtilities.GetNodeMeshPositions(
                        adjecentRoad,
                        startNode,
                        roadPosition,
                        startNodePosition,
                        out Vector3 startNodeMeshPosition,
                        out _);
                    meshData = CalculateNodeWSIMeshData.PopulateStartNode(meshData, startNodeMeshPosition, endCenterNode, startNodePosition, roadWidth, resolution);
                    return meshData;
                }
            }

            foreach (float adjecentRoadAngle in adjacentRoads.Keys) {
                adjecentRoad = adjacentRoads.GetValueOrDefault(adjecentRoadAngle);
                if (adjecentRoadAngle > 0) {
                    // road is to the left
                    MeshUtilities.GetNodeMeshPositions(
                        adjecentRoad,
                        startNode,
                        roadPosition,
                        startNodePosition,
                        out leftRoadCenter,
                        out leftRoadControlNode);
                } else {
                    // road is to the right
                    MeshUtilities.GetNodeMeshPositions(
                        adjecentRoad,
                        startNode,
                        roadPosition,
                        startNodePosition,
                        out rightRoadCenter,
                        out rightRoadControlNode);
                }

            }

            Vector3 startLeft = RoadUtilities.GetRoadRightSideVertice(roadWidth, leftRoadCenter, leftRoadControlNode);
            Vector3 endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, endCenterNode, controlPosition);

            Vector3 startRight = RoadUtilities.GetRoadLeftSideVertice(roadWidth, rightRoadCenter, rightRoadControlNode);
            Vector3 endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endCenterNode, controlPosition);

            Vector3 n0Left = (startLeft - leftRoadCenter).normalized;
            Vector3 n1Left = (endLeft - endCenterNode).normalized;

            Vector3 n0Right = (startRight - rightRoadCenter).normalized;
            Vector3 n1Right = (endRight - endCenterNode).normalized;

            if (Vector3.Angle(n0Left, n1Left) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlLeft = startNodePosition + ((n0Left + n1Left) * roadWidth) / Vector3.Dot((n0Left + n1Left), (n0Left + n1Left));
            } else {
                // Road is traight, so calculations are easier
                controlLeft = startNodePosition + n0Left * roadWidth / 2;
            }

            if (Vector3.Angle(n0Right, n1Right) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlRight = startNodePosition + ((n0Right + n1Right) * roadWidth) / Vector3.Dot((n0Right + n1Right), (n0Right + n1Right));
            } else {
                // Road is traight, so calculations are easier
                controlRight = startNodePosition + n1Right * roadWidth / 2;
            }

            meshData = MeshUtilities.PopulateStartNodeMeshVertices(
                meshData,
                resolution,
                startLeft,
                endLeft,
                controlLeft,
                startCenterNode,
                endCenterNode,
                startRight,
                endRight,
                controlRight);

            return meshData;
        }


        public MeshData PopulateEndNode(MeshData meshData) {

            roadOffsetDistance = endNode.GetNodeSizeForRoad(roadObject);
            startCenterNode = Bezier.GetOffsettedPosition(endNodePosition, startNodePosition, controlPosition, roadOffsetDistance);
            endCenterNode = endNodePosition + (endNodePosition - startCenterNode);

            // If node has no intersection just run the normal function
            if (!endNode.HasIntersection()) {
                meshData = CalculateNodeWOIMeshData.PopulateEndNode(meshData, roadWidth, endNodePosition, controlPosition, resolution);
                return meshData;
            }
            Dictionary<float, RoadObject> adjacentRoads = endNode.GetAdjacentRoadsTo(roadObject);

            if (adjacentRoads.Count == 1) {
                foreach (float adjecentRoadAngle in adjacentRoads.Keys) {
                    adjecentRoad = adjacentRoads.GetValueOrDefault(adjecentRoadAngle);
                    MeshUtilities.GetNodeMeshPositions(
                        adjecentRoad,
                        endNode,
                        roadPosition,
                        endNodePosition,
                        out leftRoadCenter,
                        out _);

                    meshData = CalculateNodeWSIMeshData.PopulateEndNode(meshData, startCenterNode, leftRoadCenter, endNodePosition, roadWidth, resolution);
                    return meshData;
                }
            }

            foreach (float adjecentRoadAngle in adjacentRoads.Keys) {
                adjecentRoad = adjacentRoads.GetValueOrDefault(adjecentRoadAngle);
                if (adjecentRoadAngle > 0) {
                    // road is to the left
                    MeshUtilities.GetNodeMeshPositions(
                        adjecentRoad,
                        endNode,
                        roadPosition,
                        endNodePosition,
                        out leftRoadCenter,
                        out leftRoadControlNode);
                } else {
                    // road is to the right
                    MeshUtilities.GetNodeMeshPositions(
                        adjecentRoad,
                        endNode,
                        roadPosition,
                        endNodePosition,
                        out rightRoadCenter,
                        out rightRoadControlNode);
                }
            }

            Vector3 leftRoadRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, leftRoadCenter, leftRoadControlNode);
            Vector3 thisRoadRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startCenterNode, controlPosition);

            Vector3 rightRoadLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, rightRoadCenter, rightRoadControlNode);
            Vector3 thisRoadLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startCenterNode, controlPosition);

            Vector3 n0Left = (leftRoadRight - leftRoadCenter).normalized;
            Vector3 n1Left = (thisRoadLeft - startCenterNode).normalized;

            Vector3 n0Right = (rightRoadLeft - rightRoadCenter).normalized;
            Vector3 n1Right = (thisRoadRight - startCenterNode).normalized;

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

            meshData = MeshUtilities.PopulateEndNodeMeshVertices(
                meshData,
                resolution,
                thisRoadRight,
                rightRoadLeft,
                controlRight,
                startCenterNode,
                endCenterNode,
                thisRoadLeft,
                leftRoadRight,
                controlLeft);

            return meshData;
        }
    }
}