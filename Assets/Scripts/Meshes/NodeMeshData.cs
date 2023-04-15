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

        private Vector3 startNodePosition = Vector3.zero;

        public NodeMeshData(Node node) 
        {
            this.node = node;
        }

        public MeshData PopulateMesh(MeshData meshData, RoadObject connectedRoad)
        {
            roadWidth = connectedRoad.RoadWidth;
            resolution = connectedRoad.RoadResolution;

            if (!node.HasIntersection())
            {
                return PopulateNodeWithoutIntersection(meshData, connectedRoad);
            }

            Dictionary<float, RoadObject> adjacentRoads = node.GetAdjacentRoadsTo(connectedRoad);
            if (adjacentRoads.Count == 1)
            {
                return PopulateNodeWithSingleIntersection(meshData, connectedRoad, adjacentRoads.First().Value);
            }

            return PopulateNodeWithDoubleIntersection(meshData, connectedRoad, adjacentRoads);
        }

        /// <summary>
        /// Populate vertices for the start Node of a Road
        /// It stats where the left and right side of the mesh
        /// meets and finish on the left and righ sides of the
        /// Road, forming a semi circle
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateNodeWithoutIntersection(MeshData meshData, RoadObject connectedRoad)
        {
            Vector3 endLeft;
            Vector3 endRight;
            Vector3 startPosition = -connectedRoad.ControlPosition(node).normalized * roadWidth / 2;

            if (connectedRoad.StartNode == node)
            {
                endLeft = connectedRoad.StartMeshLeftPosition(node);
                endRight = connectedRoad.StartMeshRightPosition(node);
            }
            else
            {
                endLeft = connectedRoad.EndMeshLeftPosition(node);
                endRight = connectedRoad.EndMeshRightPosition(node);
            }

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
        public MeshData PopulateNodeWithSingleIntersection(
            MeshData meshData, 
            RoadObject connectedRoad, 
            RoadObject adjecentRoad) 
        {
            Vector3 thisRoadCenter;
            Vector3 otherRoadCenter;

            Vector3 thisRoadRight;
            Vector3 thisRoadLeft;

            Vector3 otherRoadRight;
            Vector3 otherRoadLeft;

            if (connectedRoad.StartNode == node)
            {
                thisRoadCenter = connectedRoad.StartMeshCenterPosition(node);
                thisRoadRight = connectedRoad.StartMeshLeftPosition(node);
                thisRoadLeft = connectedRoad.StartMeshRightPosition(node); 
            }
            else
            {
                thisRoadCenter = connectedRoad.EndMeshCenterPosition(node);
                thisRoadRight = connectedRoad.EndMeshLeftPosition(node);
                thisRoadLeft = connectedRoad.EndMeshRightPosition(node); 
            }

            if (adjecentRoad.StartNode == node)
            {
                otherRoadCenter = adjecentRoad.StartMeshCenterPosition(node);
                otherRoadRight = adjecentRoad.StartMeshLeftPosition(node);
                otherRoadLeft = adjecentRoad.StartMeshRightPosition(node); 
            }
            else
            {
                otherRoadCenter = adjecentRoad.EndMeshCenterPosition(node);
                otherRoadRight = adjecentRoad.EndMeshLeftPosition(node);
                otherRoadLeft = adjecentRoad.EndMeshRightPosition(node); 
            }

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
        public MeshData PopulateNodeWithDoubleIntersection(
            MeshData meshData, 
            RoadObject connectedRoad, 
            Dictionary<float, RoadObject> adjacentRoads)
        {
            if (adjacentRoads.Count != 2)
            {
                throw new System.Exception("Intersection MUST have two roads, but has " + adjacentRoads.Count);
            }

            RoadObject leftRoad = adjacentRoads.First().Value;
            RoadObject rightRoad = adjacentRoads.Last().Value;

            Vector3 thisRoadCenter;
            Vector3 leftRoadCenter;
            Vector3 rightRoadCenter;

            Vector3 thisRoadRight;
            Vector3 thisRoadLeft;

            Vector3 leftRoadRight;            
            Vector3 rightRoadLeft;

            if (connectedRoad.StartNode == node)
            {
                thisRoadCenter = connectedRoad.StartMeshCenterPosition(node);
                thisRoadRight = connectedRoad.StartMeshRightPosition(node);
                thisRoadLeft = connectedRoad.StartMeshLeftPosition(node);
            }
            else
            {
                thisRoadCenter = connectedRoad.EndMeshCenterPosition(node);
                thisRoadRight = connectedRoad.EndMeshRightPosition(node);
                thisRoadLeft = connectedRoad.EndMeshLeftPosition(node);
            }

            if (leftRoad.StartNode == node)
            {
                leftRoadCenter = leftRoad.StartMeshCenterPosition(node);
                leftRoadRight = leftRoad.StartMeshRightPosition(node);
            }
            else
            {
                leftRoadCenter = leftRoad.EndMeshCenterPosition(node);
                leftRoadRight = leftRoad.EndMeshRightPosition(node);
            }

            if (rightRoad.StartNode == node)
            {
                rightRoadCenter = rightRoad.StartMeshCenterPosition(node);
                rightRoadLeft = rightRoad.StartMeshLeftPosition(node);
            }
            else
            {
                rightRoadCenter = rightRoad.EndMeshCenterPosition(node);
                rightRoadLeft = rightRoad.EndMeshLeftPosition(node);
            }

            Vector3 intersectionCenter = startNodePosition + (startNodePosition - thisRoadCenter);

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
