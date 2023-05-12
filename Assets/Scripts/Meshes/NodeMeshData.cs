using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Roads.Utilities;
using Roads;
using Roads.MeshHandler;
using UnityEngine.SocialPlatforms.GameCenter;
using Rafael.Utils;

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

            List<RoadObject> adjacentRoads = node.GetAdjacentRoadsTo(connectedRoad).Values.ToList<RoadObject>();
            if (adjacentRoads.Count == 1)
            {
                return PopulateNodeWithSingleIntersection(meshData, connectedRoad, adjacentRoads.First());
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
            MeshEdje roadCenter;
            MeshEdje roadRight;
            MeshEdje roadLeft;

            if (connectedRoad.StartNode == node)
            {
                roadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartCenter);
                roadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartLeft);
                roadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartRight);

            }
            else
            {
                roadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndCenter);
                roadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndLeft);
                roadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndRight);
            }

            Vector3 roadCenterPos = roadCenter.Position - node.Position;
            Vector3 roadLeftPos = roadLeft.Position - node.Position;
            Vector3 roadRightPos = roadRight.Position - node.Position;

            Vector3 startPosition = roadCenterPos - roadCenterPos.normalized * roadWidth / 2;
            Vector3 leftDir = (roadLeftPos - roadCenterPos).normalized;
            Vector3 controlLeft = startPosition + leftDir * roadWidth / 2;
            Vector3 controlRight = startPosition - leftDir * roadWidth / 2;

            MeshUtilities.PopulateStartNodeVerticesWOIntersection(
                meshData,
                resolution,
                startPosition,
                roadLeftPos,
                controlLeft,
                roadCenterPos,
                roadRightPos,
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
            MeshEdje thisRoadCenter;
            MeshEdje otherRoadCenter;

            MeshEdje thisRoadRight;
            MeshEdje thisRoadLeft;

            MeshEdje otherRoadRight;
            MeshEdje otherRoadLeft;

            if (connectedRoad.StartNode == node)
            {
                thisRoadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartCenter);
                thisRoadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartLeft);
                thisRoadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartRight);

            }
            else
            {
                thisRoadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndCenter);
                thisRoadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndLeft);
                thisRoadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndRight);
            }

            if (adjacentRoad.StartNode == node)
            {
                otherRoadCenter = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.StartCenter);
                otherRoadLeft = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.StartLeft);
                otherRoadRight = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.StartRight);

            }
            else
            {
                otherRoadCenter = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.EndCenter);
                otherRoadLeft = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.EndLeft);
                otherRoadRight = node.GetMeshEdjeFor(adjacentRoad, MeshEdje.EdjePosition.EndRight);
            }

            Vector3 thisRoadCenterPos = thisRoadCenter.Position - node.Position;
            Vector3 thisRoadLeftPos = thisRoadLeft.Position - node.Position;
            Vector3 thisRoadRightPos = thisRoadRight.Position - node.Position;

            Vector3 otherRoadCenterPos = otherRoadCenter.Position - node.Position;
            Vector3 otherRoadLeftPos = otherRoadLeft.Position - node.Position;
            Vector3 otherRoadRightPos = otherRoadRight.Position - node.Position;

            RafaelUtils.LineLineIntersection(out Vector3 controlLeftPos,
                thisRoadRightPos,
                thisRoadRight.Direction,
                otherRoadLeftPos,
                otherRoadLeft.Direction);
            RafaelUtils.LineLineIntersection(
                out Vector3 controlRightPos,
                thisRoadLeftPos,
                thisRoadLeft.Direction,
                otherRoadRightPos,
                otherRoadRight.Direction);


            meshData = MeshUtilities.PopulateNodeMeshVerticesWSIntersection(
                meshData,
                resolution,
                thisRoadRightPos,
                otherRoadLeftPos,
                controlLeftPos,
                thisRoadCenterPos,
                otherRoadCenterPos,
                startNodePosition,
                thisRoadLeftPos,
                otherRoadRightPos,
                controlRightPos);

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
            List<RoadObject> adjacentRoads)
        {
            if (adjacentRoads.Count != 2)
            {
                throw new System.Exception("Intersection MUST have two roads, but has " + adjacentRoads.Count);
            }

            RoadObject leftRoad = adjacentRoads.First();
            RoadObject rightRoad = adjacentRoads.Last();

            MeshEdje thisRoadCenter;
            MeshEdje thisRoadRight;
            MeshEdje thisRoadLeft;

            MeshEdje leftRoadRight;
            MeshEdje rightRoadLeft;

            if (connectedRoad.StartNode == node)
            {
                thisRoadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartCenter);
                thisRoadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartLeft);
                thisRoadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.StartRight);
            }
            else
            {
                thisRoadCenter = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndCenter);
                thisRoadLeft = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndLeft);
                thisRoadRight = node.GetMeshEdjeFor(connectedRoad, MeshEdje.EdjePosition.EndRight);
            }

            if (leftRoad.StartNode == node)
                leftRoadRight = node.GetMeshEdjeFor(leftRoad, MeshEdje.EdjePosition.StartRight);
            else
                leftRoadRight = node.GetMeshEdjeFor(leftRoad, MeshEdje.EdjePosition.EndRight);

            if (rightRoad.StartNode == node)
                rightRoadLeft = node.GetMeshEdjeFor(rightRoad, MeshEdje.EdjePosition.StartLeft);
            else
                rightRoadLeft = node.GetMeshEdjeFor(rightRoad, MeshEdje.EdjePosition.EndLeft);

            Vector3 thisRoadCenterPos = thisRoadCenter.Position - node.Position;
            Vector3 thisRoadLeftPos = thisRoadLeft.Position - node.Position;
            Vector3 thisRoadRightPos = thisRoadRight.Position - node.Position;
            Vector3 leftRoadRightPos = leftRoadRight.Position - node.Position;
            Vector3 rightRoadLeftPos = rightRoadLeft.Position - node.Position;

            Vector3 intersectionCenter = -thisRoadCenterPos;

            RafaelUtils.LineLineIntersection(
                out Vector3 controlLeftPos,
                leftRoadRightPos,
                leftRoadRight.Direction,
                thisRoadLeftPos,
                thisRoadLeft.Direction);
            RafaelUtils.LineLineIntersection(
                out Vector3 controlRightPos,
                rightRoadLeftPos,
                rightRoadLeft.Direction,
                thisRoadRightPos,
                thisRoadRight.Direction);

            meshData = MeshUtilities.PopulateNodeMeshVerticesWDIntersection(
                meshData,
                resolution,

                leftRoadRightPos,
                thisRoadLeftPos,
                controlLeftPos,

                intersectionCenter,
                thisRoadCenterPos,

                rightRoadLeftPos,
                thisRoadRightPos,
                controlRightPos);

            return meshData;
        }
    }
}
