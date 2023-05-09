using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Roads.Utilities;
using Roads;
using Roads.MeshHandler;
using UnityEngine.SocialPlatforms.GameCenter;

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

            if (!node.HasIntersection)
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
            Vector3 center;
            Vector3 left;
            Vector3 right;

            if (connectedRoad.StartNode == node)
            {
                center = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartCenter).transform.position - node.Position;
                left = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartLeft).transform.position - node.Position;
                right = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartRight).transform.position - node.Position;
            }
            else
            {
                center = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndCenter).transform.position - node.Position;
                left = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndLeft).transform.position - node.Position;
                right = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndRight).transform.position - node.Position;
            }

            Vector3 startPosition = center - center.normalized * roadWidth / 2;
            Vector3 leftDir = (left - center).normalized;
            Vector3 controlLeft = startPosition + leftDir * roadWidth / 2;
            Vector3 controlRight = startPosition - leftDir * roadWidth / 2;

            MeshUtilities.PopulateStartNodeVerticesWOIntersection(
                meshData,
                resolution,
                startPosition,
                left,
                controlLeft,
                center,
                right,
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
            RoadObject adjacentRoad) 
        {
            Vector3 thisRoadCenter;
            Vector3 otherRoadCenter;

            Vector3 thisRoadRight;
            Vector3 thisRoadLeft;

            Vector3 otherRoadRight;
            Vector3 otherRoadLeft;

            if (connectedRoad.StartNode == node)
            {
                thisRoadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartCenter).transform.position - node.Position;
                thisRoadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartLeft).transform.position - node.Position;
                thisRoadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartRight).transform.position - node.Position;

            }
            else
            {
                thisRoadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndCenter).transform.position - node.Position;
                thisRoadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndLeft).transform.position - node.Position;
                thisRoadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndRight).transform.position - node.Position;
            }

            if (adjacentRoad.StartNode == node)
            {
                otherRoadCenter = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.StartCenter).transform.position - node.Position;
                otherRoadLeft = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.StartLeft).transform.position - node.Position;
                otherRoadRight = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.StartRight).transform.position - node.Position;

            }
            else
            {
                otherRoadCenter = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.EndCenter).transform.position - node.Position;
                otherRoadLeft = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.EndLeft).transform.position - node.Position;
                otherRoadRight = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.EndRight).transform.position - node.Position;
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
                thisRoadRight,
                otherRoadLeft,
                controlLeft,
                thisRoadCenter,
                otherRoadCenter,
                startNodePosition,
                thisRoadLeft,
                otherRoadRight,
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
                thisRoadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartCenter).transform.position - node.Position;
                thisRoadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartLeft).transform.position - node.Position;
                thisRoadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartRight).transform.position - node.Position;
            }
            else
            {
                thisRoadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndCenter).transform.position - node.Position;
                thisRoadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndLeft).transform.position - node.Position;
                thisRoadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndRight).transform.position - node.Position;
            }

            if (leftRoad.StartNode == node)
            {
                leftRoadCenter = node.GetMeshEdjeFor(leftRoad, MeshEdje.EdjePosition.StartCenter).transform.position - node.Position;
                leftRoadRight = node.GetMeshEdjeFor(leftRoad, MeshEdje.EdjePosition.StartRight).transform.position - node.Position;
            }
            else
            {
                leftRoadCenter = node.GetMeshEdjeFor(leftRoad, MeshEdje.EdjePosition.EndCenter).transform.position - node.Position;
                leftRoadRight = node.GetMeshEdjeFor(leftRoad, MeshEdje.EdjePosition.EndRight).transform.position - node.Position;
            }

            if (rightRoad.StartNode == node)
            {
                rightRoadCenter = node.GetMeshEdjeFor(rightRoad, MeshEdje.EdjePosition.StartCenter).transform.position - node.Position;
                rightRoadLeft = node.GetMeshEdjeFor(rightRoad, MeshEdje.EdjePosition.StartLeft).transform.position - node.Position;
            }
            else
            {
                rightRoadCenter = node.GetMeshEdjeFor(rightRoad, MeshEdje.EdjePosition.EndCenter).transform.position - node.Position;
                rightRoadLeft = node.GetMeshEdjeFor(rightRoad, MeshEdje.EdjePosition.EndLeft).transform.position - node.Position;
            }

            Vector3 intersectionCenter = -thisRoadCenter;

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
