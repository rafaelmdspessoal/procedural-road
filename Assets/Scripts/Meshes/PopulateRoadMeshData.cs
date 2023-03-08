using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Road.Mesh.Data {
    public class PopulateRoadMeshData {

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

        private static Vector3 startLeft;
        private static Vector3 endRight;

        private static Vector3 startRight;
        private static Vector3 endLeft;

        private static Vector3 n0;
        private static Vector3 n1;

        private static Vector3 n0Left;
        private static Vector3 n1Left;

        private static Vector3 n0Right;
        private static Vector3 n1Right;

        /// <summary>
        /// Generates the geometry for the given Road Object
        /// </summary>
        /// <param name="roadObject"></param>
        public PopulateRoadMeshData(RoadObject roadObject) {
            this.roadObject = roadObject;

            startNode = roadObject.StartNode;
            endNode = roadObject.EndNode;
            roadPosition = roadObject.transform.position;
            startNodePosition = startNode.Position - roadPosition;
            endNodePosition = endNode.Position - roadPosition;
            controlPosition = roadObject.ControlNodeObject.transform.position - roadPosition;
            roadWidth = roadObject.RoadWidth;
            resolution = roadObject.RoadResolution * 3;
        }

        /// <summary>
        /// Populate mesh for given Road Object
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateRoadMeshVertices(MeshData meshData) {

            startCenterNode = startNodePosition;
            endCenterNode = endNodePosition;

            if (startNode.HasIntersection()) {
                roadOffsetDistance = startNode.GetNodeSizeForRoad(roadObject);
                startCenterNode = Bezier.GetOffsettedPosition(startNodePosition, endNodePosition, controlPosition, roadOffsetDistance);
            }
            if (endNode.HasIntersection()) {
                roadOffsetDistance = endNode.GetNodeSizeForRoad(roadObject);
                endCenterNode = Bezier.GetOffsettedPosition(endNodePosition, startNodePosition, controlPosition, roadOffsetDistance);
            }

            CalculateRoadMeshConnections(startCenterNode, endCenterNode, controlPosition);

            meshData = MeshUtilities.PopulateRoadMeshVertices(
                meshData,
                resolution,
                startLeft,
                endLeft,
                controlLeft,
                startCenterNode,
                endCenterNode,
                controlPosition,
                startRight,
                endRight,
                controlRight);

            return meshData;
        }

        /// <summary>
        /// Populates the starting Node mesh for the given Road Object
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateStartNodeMesh(MeshData meshData) {

            Dictionary<float, RoadObject> adjacentRoads = startNode.GetAdjacentRoadsTo(roadObject);
            roadOffsetDistance = startNode.GetNodeSizeForRoad(roadObject);
            endCenterNode = Bezier.GetOffsettedPosition(startNodePosition, endNodePosition, controlPosition, roadOffsetDistance);
            startCenterNode = startNodePosition + (startNodePosition - endCenterNode);

            // If node has no intersection just run the normal function
            if (!startNode.HasIntersection()) {
                CalculateNodeWOIMeshData.PopulateStartNode(meshData, roadWidth, startNodePosition, controlPosition, resolution);
                return meshData;
            }

            if (adjacentRoads.Count == 1) {
                foreach (float adjecentRoadAngle in adjacentRoads.Keys) {
                    adjecentRoad = adjacentRoads.GetValueOrDefault(adjecentRoadAngle);
                    MeshUtilities.GetNodeMeshPositions(
                        adjecentRoad,
                        startNode,
                        roadPosition,
                        startNodePosition,
                        out Vector3 otherRoadCenter,
                        out _);
                    //Xuxa(otherRoadCenter, endCenterNode, startNodePosition);
                    //meshData = MeshUtilities.PopulateStartNodeMeshVertices(
                    //    meshData,
                    //    resolution,
                    //    startLeft,
                    //    endLeft,
                    //    controlLeft,
                    //    otherRoadCenter,
                    //    endCenterNode,
                    //    controlPosition,
                    //    startRight,
                    //    endRight,
                    //    controlRight);
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

            CalculateNodeMeshConnections(endCenterNode, startNodePosition);

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

        /// <summary>
        /// Populate the end Node mesh for the given Road Object
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateEndNodeMesh(MeshData meshData) {

            Dictionary<float, RoadObject> adjacentRoads = endNode.GetAdjacentRoadsTo(roadObject);
            roadOffsetDistance = endNode.GetNodeSizeForRoad(roadObject);
            startCenterNode = Bezier.GetOffsettedPosition(endNodePosition, startNodePosition, controlPosition, roadOffsetDistance);
            endCenterNode = endNodePosition + (endNodePosition - startCenterNode);

            // If node has no intersection just run the normal function
            if (!endNode.HasIntersection()) {
                meshData = CalculateNodeWOIMeshData.PopulateEndNode(meshData, roadWidth, endNodePosition, controlPosition, resolution);
                return meshData;
            }

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

            CalculateNodeMeshConnections(startCenterNode, endNodePosition);

            meshData = MeshUtilities.PopulateEndNodeMeshVertices(
                meshData,
                resolution,
                endRight,
                startRight,
                controlRight,
                startCenterNode,
                endCenterNode,
                endLeft,
                startLeft,
                controlLeft);

            return meshData;
        }

        /// <summary>
        /// Calculate where each node for each road will be
        /// </summary>
        private void CalculateNodeMeshConnections(Vector3 centerNode, Vector3 nodePosition) {
            startLeft = RoadUtilities.GetRoadRightSideVertice(roadWidth, leftRoadCenter, leftRoadControlNode);
            endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, centerNode, controlPosition);

            startRight = RoadUtilities.GetRoadLeftSideVertice(roadWidth, rightRoadCenter, rightRoadControlNode);
            endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, centerNode, controlPosition);

            n0Left = (startLeft - leftRoadCenter).normalized;
            n1Left = (endLeft - centerNode).normalized;

            n0Right = (startRight - rightRoadCenter).normalized;
            n1Right = (endRight - centerNode).normalized;

            if (Vector3.Angle(n0Left, n1Left) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlLeft = nodePosition + ((n0Left + n1Left) * roadWidth) / Vector3.Dot((n0Left + n1Left), (n0Left + n1Left));
            } else {
                // Road is traight, so calculations are easier
                controlLeft = nodePosition + n0Left * roadWidth / 2;
            }

            if (Vector3.Angle(n0Right, n1Right) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlRight = nodePosition + ((n0Right + n1Right) * roadWidth) / Vector3.Dot((n0Right + n1Right), (n0Right + n1Right));
            } else {
                // Road is traight, so calculations are easier
                controlRight = nodePosition + n1Right * roadWidth / 2;
            }
        }

        private void CalculateRoadMeshConnections(Vector3 centerNode, Vector3 nodePosition, Vector3 controlPosition) {

            startLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, centerNode, controlPosition);
            endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, nodePosition, controlPosition);

            startRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, centerNode, controlPosition);
            endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, nodePosition, controlPosition);

            n0 = (startLeft - centerNode).normalized;
            n1 = (endRight - nodePosition).normalized;

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
        }
        public void Xuxa(Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition) {
            startLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startPosition, controlPosition);
            endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endPosition, controlPosition);

            startRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startPosition, controlPosition);
            endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, endPosition, controlPosition);

            n0 = (startLeft - startPosition).normalized;
            n1 = (endRight - endPosition).normalized;

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
        }
    }
}