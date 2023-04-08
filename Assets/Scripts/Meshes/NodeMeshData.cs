using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Roads.Utilities;
using Roads;
using Roads.MeshHandler;

namespace Nodes.MeshHandler.Data {
    public class NodeMeshData {

        private readonly Node node;

        private int resolution;
        private int roadWidth;

        private Vector3 roadPosition;
        private Vector3 startNodePosition = Vector3.zero;
        private Vector3 endNodePosition;
        private Vector3 controlPosition;

        public NodeMeshData(Node node) 
        {
            this.node = node;
        }

        public MeshData PopulateMesh(MeshData meshData, RoadObject connectedRoad)
        {
            roadPosition = connectedRoad.transform.position;
            controlPosition = connectedRoad.ControlNodeObject.transform.position - node.Position;
            endNodePosition = connectedRoad.OtherNodeTo(node).Position - node.Position;

            roadWidth = connectedRoad.RoadWidth;
            resolution = connectedRoad.RoadResolution;

            if (!node.HasIntersection())
            {
                return PopulateNodeWithoutIntersection(meshData);
            }

            float roadOffsetDistance = node.GetNodeSizeForRoad(connectedRoad);
            Vector3 startCenterNode = Bezier.GetOffsettedPosition(startNodePosition, endNodePosition, controlPosition, roadOffsetDistance);
            Dictionary<float, RoadObject> adjacentRoads = node.GetAdjacentRoadsTo(connectedRoad);
            if (adjacentRoads.Count == 1)
            {
                return PopulateNodeWithSingleIntersection(meshData, adjacentRoads.First().Value, startCenterNode);
            }

            return PopulateNodeWithDoubleIntersection(meshData, adjacentRoads, startCenterNode);
        }

        /// <summary>
        /// Populate vertices for the start Node of a Road
        /// It stats where the left and right side of the mesh
        /// meets and finish on the left and righ sides of the
        /// Road, forming a semi circle
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateNodeWithoutIntersection(MeshData meshData) {
            Vector3 startPosition = startNodePosition + (startNodePosition - controlPosition).normalized * roadWidth / 2;
            Vector3 endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startNodePosition, controlPosition);
            Vector3 endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startNodePosition, controlPosition);

            Vector3 left = (endLeft - startNodePosition).normalized;
            Vector3 controlLeft = startPosition + left * roadWidth / 2;
            Vector3 controlRight = startPosition - left * roadWidth / 2;

            MeshUtilities.PopulateStartNodeVerticesWOIntersection(
                meshData,
                resolution,
                startPosition,
                endLeft,
                controlLeft,
                startNodePosition,
                endRight,
                controlRight);
           
            return meshData;
        }

        /// <summary>
        /// Populate vertices for the start Node of a Road
        /// For this the left side of each road is connecte
        /// to the right side of the next one
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="adjacentRoad"></param>
        /// <returns></returns>
        public MeshData PopulateNodeWithSingleIntersection(MeshData meshData, RoadObject adjecentRoad, Vector3 thisRoadCenter) {

            MeshUtilities.GetNodeMeshPositions(
                adjecentRoad,
                node,
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

            meshData = MeshUtilities.PopulateNodeMeshVerticesWSIntersection(
                meshData,
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
        public MeshData PopulateNodeWithDoubleIntersection(MeshData meshData, Dictionary<float, RoadObject> adjacentRoads, Vector3 thisRoadCenter) {
            if(adjacentRoads.Count != 2) {
                throw new System.Exception("Intersection MUST have two roads, but has " + adjacentRoads.Count);
            }
            Vector3 intersectionCenter = startNodePosition + (startNodePosition - thisRoadCenter);

            MeshUtilities.GetNodeMeshPositions(
                adjacentRoads.First().Value,
                node,
                roadPosition,
                startNodePosition,
                out Vector3 leftRoadCenter,
                out _);
            MeshUtilities.GetNodeMeshPositions(
                adjacentRoads.Last().Value,
                node,
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

            meshData = MeshUtilities.PopulateNodeMeshVerticesWDIntersection(
                meshData,
                resolution,
                leftRoadRight,
                thisRoadLeft,
                controlLeft,
                intersectionCenter,
                thisRoadCenter,
                rightRoadLeft,
                thisRoadRight,
                controlRight);
            return meshData;
        }
    }
}
