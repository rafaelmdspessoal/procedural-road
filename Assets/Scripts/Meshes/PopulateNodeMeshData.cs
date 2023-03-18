using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MeshHandler.Utilities;
using Rafael.Utils;

namespace MeshHandler.Road.NodeMeshData {
    public class PopulateNodeMeshData {

        private readonly RoadObject roadObject;
        private readonly Node startNode;
        private readonly Node endNode;

        private readonly int resolution;
        private readonly int roadWidth;

        private Vector3 roadPosition;
        private Vector3 startNodePosition;
        private Vector3 endNodePosition;
        private Vector3 controlPosition;

        /// <summary>
        /// Generates the geometry for the given Road Object
        /// </summary>
        /// <param name="roadObject"></param>
        public PopulateNodeMeshData(RoadObject roadObject) {
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

        public MeshData PopulateStartNodeMesh(MeshData meshData) {
            if (!startNode.HasIntersection()) {
                return PopulateStartNodeWithoutIntersection(meshData);
            }

            float roadOffsetDistance = startNode.GetNodeSizeForRoad(roadObject);
            Vector3 startCenterNode = Bezier.GetOffsettedPosition(startNodePosition, endNodePosition, controlPosition, roadOffsetDistance);
            Dictionary<float, RoadObject> adjacentRoads = startNode.GetAdjacentRoadsTo(roadObject);
            if (adjacentRoads.Count == 1) {
                return PopulateStartNodeWithSingleIntersection(meshData, adjacentRoads.First().Value, startCenterNode);
            }

            return PopulateStartNodeWithDoubleIntersection(meshData, adjacentRoads, startCenterNode);
        }

        public MeshData PopulateEndNodeMesh(MeshData meshData) {
            if (!endNode.HasIntersection()) {
                return PopulateEndNodeWithoutIntersection(meshData);
            }

            float roadOffsetDistance = endNode.GetNodeSizeForRoad(roadObject);
            Vector3 endCenterNode = Bezier.GetOffsettedPosition(endNodePosition, startNodePosition, controlPosition, roadOffsetDistance);
            Dictionary<float, RoadObject> adjacentRoads = endNode.GetAdjacentRoadsTo(roadObject);
            if (adjacentRoads.Count == 1) {
                return PopulateEndNodeWithSingleIntersection(meshData, adjacentRoads.First().Value, endCenterNode);
            }

            return PopulateEndNodeWithDoubleIntersection(meshData, adjacentRoads, endCenterNode);
        }

        #region NoIntersectionNode
        /// <summary>
        /// Populate vertices for the start Node of a Road
        /// It stats where the left and right side of the mesh
        /// meets and finish on the left and righ sides of the
        /// Road, forming a semi circle
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateStartNodeWithoutIntersection(MeshData meshData) {
            Vector3 startPosition = startNodePosition + (startNodePosition - controlPosition).normalized * roadWidth / 2;

            Vector3 endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startNodePosition, controlPosition);
            Vector3 endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startNodePosition, controlPosition);

            Vector3 left = (endLeft - startNodePosition).normalized;
            Vector3 controlLeft = startPosition + left * roadWidth / 2;
            Vector3 controlRight = startPosition - left * roadWidth / 2;

            MeshUtilities utilities = new(
                resolution,
                startPosition,
                endLeft,
                controlLeft,

                startNodePosition,
                startNodePosition,
                startNodePosition,

                startPosition,
                endRight,
                controlRight);

            meshData = utilities.PopulateStartNodeVerticesWOIntersection(meshData);
            return meshData;
        }

        /// <summary>
        /// Populate vertices for the start Node of a Road
        /// It stats on the left and righ sides of the
        /// Road and finish where the left and right side 
        /// of the mesh meets, forming a semi circle.
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateEndNodeWithoutIntersection(MeshData meshData) {
            Vector3 endPosition = endNodePosition + (endNodePosition - controlPosition).normalized * roadWidth / 2;

            Vector3 startRight = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endNodePosition, controlPosition);
            Vector3 startLeft = RoadUtilities.GetRoadRightSideVertice(roadWidth, endNodePosition, controlPosition);

            Vector3 left = (startLeft - endNodePosition).normalized;
            Vector3 controlLeft = endPosition + left * roadWidth / 2;
            Vector3 controlRight = endPosition - left * roadWidth / 2;

            MeshUtilities utilities = new(
               resolution,
               startLeft,
               endPosition,
               controlLeft,

               endNodePosition,
               endNodePosition,
               endNodePosition,

               startRight,
               endPosition,
               controlRight);

            meshData = utilities.PopulateEndNodeVerticesWOIntersection(meshData);
            return meshData;
        }
        #endregion

        #region SingleInterserctionNode
        /// <summary>
        /// Populate vertices for the start Node of a Road
        /// For this the left side of each road is connecte
        /// to the right side of the next one
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="adjacentRoad"></param>
        /// <returns></returns>
        public MeshData PopulateStartNodeWithSingleIntersection(MeshData meshData, RoadObject adjecentRoad, Vector3 thisRoadCenter) {

            MeshUtilities.GetNodeMeshPositions(
                adjecentRoad,
                startNode,
                roadPosition,
                startNodePosition,
                out Vector3 otherRoadCenter,
                out _);

            Vector3 otherRoadLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, otherRoadCenter, startNodePosition);
            Vector3 thisRoadRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, thisRoadCenter, startNodePosition);

            Vector3 otherRoadRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, otherRoadCenter, startNodePosition);
            Vector3 thisRoadLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, thisRoadCenter, startNodePosition);

            Vector3 n0 = (otherRoadLeft - otherRoadCenter).normalized;
            Vector3 n1 = (thisRoadRight - thisRoadCenter).normalized;

            Vector3 controlLeft;
            Vector3 controlRight;


            if (Vector3.Angle(n0, n1) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlLeft = startNodePosition + ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
                controlRight = startNodePosition - ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
            } else {
                // Road is traight, so calculations are easier
                controlLeft = startNodePosition + n0 * roadWidth / 2;
                controlRight = startNodePosition - n1 * roadWidth / 2;
            }

            MeshUtilities utilities = new(
               resolution,
               otherRoadLeft,
               thisRoadRight,
               controlLeft,

               otherRoadCenter,
               thisRoadCenter,
               startNodePosition,

               otherRoadRight,
               thisRoadLeft,
               controlRight);

            meshData = utilities.PopulateStartNodeMeshVerticesWSIntersection(meshData);

            return meshData;
        }

        /// <summary>
        /// Populate vertices for the end Node of a Road
        /// For this the left side of each road is connecte
        /// to the right side of the next one
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="adjacentRoad"></param>
        /// <returns></returns>
        public MeshData PopulateEndNodeWithSingleIntersection(MeshData meshData, RoadObject adjecentRoad, Vector3 thisRoadCenter) {

            MeshUtilities.GetNodeMeshPositions(
                adjecentRoad,
                endNode,
                roadPosition,
                endNodePosition,
                out Vector3 otherRoadCenter,
                out _);

            Vector3 thisRoadRight = RoadUtilities.GetRoadLeftSideVertice(roadWidth, thisRoadCenter, endNodePosition);
            Vector3 otherRoadLeft = RoadUtilities.GetRoadRightSideVertice(roadWidth, otherRoadCenter, endNodePosition);

            Vector3 thisRoadLeft = RoadUtilities.GetRoadRightSideVertice(roadWidth, thisRoadCenter, endNodePosition);
            Vector3 otherRoadRight = RoadUtilities.GetRoadLeftSideVertice(roadWidth, otherRoadCenter, endNodePosition);

            Vector3 n0 = (thisRoadRight - thisRoadCenter).normalized;
            Vector3 n1 = (otherRoadLeft - otherRoadCenter).normalized;

            Vector3 controlLeft;
            Vector3 controlRight;

            if (Vector3.Angle(n0, n1) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlRight= endNodePosition - ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
                controlLeft = endNodePosition + ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
            } else {
                // Road is traight, so calculations are easier
                controlRight = endNodePosition - n0 * roadWidth / 2;
                controlLeft = endNodePosition + n1 * roadWidth / 2;
            }

            MeshUtilities utilities = new(
               resolution,

               thisRoadRight,
               otherRoadLeft,
               controlLeft,

               thisRoadCenter,
               otherRoadCenter,
               endNodePosition,

               thisRoadLeft,
               otherRoadRight,
               controlRight);

            meshData = utilities.PopulateEndtNodeMeshVerticesWSIntersection(meshData);

            return meshData;
        }
        #endregion

        #region DoubleIntersecionNode
        /// <summary>
        /// Populate vertices for the start Node of a Road
        /// For this the left side of the road connects to
        /// the right side of the left road and the right
        /// side connects to the left side of the right road
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="adjacentRoads"></param>
        /// <returns></returns>
        public MeshData PopulateStartNodeWithDoubleIntersection(MeshData meshData, Dictionary<float, RoadObject> adjacentRoads, Vector3 thisRoadCenter) {
            if(adjacentRoads.Count != 2) {
                throw new System.Exception("Intersection MUST have two roads, but has " + adjacentRoads.Count);
            }
            Vector3 intersectionCenter = startNodePosition + (startNodePosition - thisRoadCenter);

            MeshUtilities.GetNodeMeshPositions(
                adjacentRoads.First().Value,
                startNode,
                roadPosition,
                startNodePosition,
                out Vector3 leftRoadCenter,
                out _);
            MeshUtilities.GetNodeMeshPositions(
                adjacentRoads.Last().Value,
                startNode,
                roadPosition,
                startNodePosition,
                out Vector3 rightRoadCenter,
                out _);

            Vector3 leftRoadRight = RoadUtilities.GetRoadLeftSideVertice(roadWidth, leftRoadCenter, startNodePosition);
            Vector3 thisRoadLeft = RoadUtilities.GetRoadRightSideVertice(roadWidth, thisRoadCenter, startNodePosition);

            Vector3 rightRoadLeft = RoadUtilities.GetRoadRightSideVertice(roadWidth, rightRoadCenter, startNodePosition);
            Vector3 thisRoadRight = RoadUtilities.GetRoadLeftSideVertice(roadWidth, thisRoadCenter, startNodePosition);

            Vector3 n0Left = (leftRoadRight - leftRoadCenter).normalized;
            Vector3 n1Left = (thisRoadLeft - thisRoadCenter).normalized;

            Vector3 n0Right = (rightRoadLeft - rightRoadCenter).normalized;
            Vector3 n1Right = (thisRoadRight - thisRoadCenter).normalized;

            Vector3 controlLeft;
            Vector3 controlRight;

            if (Vector3.Angle(n0Left, n1Left) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlLeft = startNodePosition + ((n0Left + n1Left) * roadWidth) / Vector3.Dot((n0Left + n1Left), (n0Left + n1Left));
            } else {
                // Road is traight, so calculations are easier
                controlLeft = startNodePosition + n1Left * roadWidth / 2;
            }

            if (Vector3.Angle(n0Right, n1Right) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlRight = startNodePosition + ((n0Right + n1Right) * roadWidth) / Vector3.Dot((n0Right + n1Right), (n0Right + n1Right));
            } else {
                // Road is traight, so calculations are easier
                controlRight = startNodePosition + n1Right * roadWidth / 2;
            }

            MeshUtilities utilities = new(
               resolution,
               leftRoadRight,
               thisRoadLeft,
               controlLeft,

               intersectionCenter,
               thisRoadCenter,
               startNodePosition,

               rightRoadLeft,
               thisRoadRight,
               controlRight);

            meshData = utilities.PopulateStartNodeMeshVerticesWDIntersection(meshData);
            return meshData;
        }

        /// <summary>
        /// Populate vertices for the start Node of a Road
        /// For this the left side of the road connects to
        /// the right side of the left road and the right
        /// side connects to the left side of the right road
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="adjacentRoads"></param>
        /// <returns></returns>
        public MeshData PopulateEndNodeWithDoubleIntersection(MeshData meshData, Dictionary<float, RoadObject> adjacentRoads, Vector3 thisRoadCenter) {
            if (adjacentRoads.Count > 3) {
                throw new System.Exception("Intesection has more than 3 meeting roads");
            }

            Vector3 intersectionCenter = endNodePosition + (endNodePosition - thisRoadCenter);

            MeshUtilities.GetNodeMeshPositions(
                adjacentRoads.First().Value,
                endNode,
                roadPosition,
                endNodePosition,
                out Vector3 leftRoadCenter,
                out _);
            MeshUtilities.GetNodeMeshPositions(
                adjacentRoads.Last().Value,
                endNode,
                roadPosition,
                endNodePosition,
                out Vector3 rightRoadCenter,
                out _);

            Vector3 leftRoadRight = RoadUtilities.GetRoadLeftSideVertice(roadWidth, leftRoadCenter, endNodePosition);
            Vector3 thisRoadLeft = RoadUtilities.GetRoadRightSideVertice(roadWidth, thisRoadCenter, endNodePosition);

            Vector3 rightRoadLeft = RoadUtilities.GetRoadRightSideVertice(roadWidth, rightRoadCenter, endNodePosition);
            Vector3 thisRoadRight = RoadUtilities.GetRoadLeftSideVertice(roadWidth, thisRoadCenter, endNodePosition);

            Vector3 LRR = (leftRoadRight - leftRoadCenter).normalized;
            Vector3 TRL = (thisRoadLeft - thisRoadCenter).normalized;

            Vector3 RRL = (rightRoadLeft - rightRoadCenter).normalized;
            Vector3 TRR = (thisRoadRight - thisRoadCenter).normalized;

            Vector3 controlLeft;
            Vector3 controlRight;

            if (Vector3.Angle(LRR, TRL) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlLeft = endNodePosition + ((LRR + TRL) * roadWidth) / Vector3.Dot((LRR + TRL), (LRR + TRL));
            } else {
                // Road is traight, so calculations are easier
                controlLeft = endNodePosition + TRL * roadWidth / 2;
            }

            if (Vector3.Angle(TRR, RRL) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlRight = endNodePosition + ((TRR + RRL) * roadWidth) / Vector3.Dot((TRR + RRL), (TRR + RRL));
            } else {
                // Road is traight, so calculations are easier
                controlRight = endNodePosition + TRR * roadWidth / 2;
            }

            MeshUtilities utilities = new(
               resolution,
               thisRoadRight,
               rightRoadLeft,
               controlRight,
               thisRoadCenter,
               intersectionCenter,
               endNodePosition,
               thisRoadLeft,
               leftRoadRight,
               controlLeft);

            meshData = utilities.PopulateEndNodeMeshVerticesWDIntersection(meshData);
            return meshData;
        }
        #endregion
    }
}
