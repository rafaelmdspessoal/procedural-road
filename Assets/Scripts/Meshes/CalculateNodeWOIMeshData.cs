using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Road.Mesh.Data {
    public class CalculateNodeWOIMeshData {
        /// <summary>
        /// Populate vertes for the end Node of a road
        /// assuming that it has no intersection.
        /// It will start at the left and right points 
        /// and move towards a common center point at the end
        /// of the Node
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="roadWidth"></param>
        /// <param name="nodePosition"></param>
        /// <param name="controlPosition"></param>
        /// <param name="resolution"></param>
        public static MeshData PopulateEndNode(MeshData meshData, int roadWidth, Vector3 nodePosition, Vector3 controlPosition, int resolution) {
            Vector3 endPosition = nodePosition + (nodePosition - controlPosition).normalized * roadWidth / 2;

            // Because this is the last node left and right get inverted
            Vector3 rightStartPosition = RoadUtilities.GetRoadLeftSideVertice(roadWidth, nodePosition, controlPosition);
            Vector3 leftStartPosition = RoadUtilities.GetRoadRightSideVertice(roadWidth, nodePosition, controlPosition);

            Vector3 left = (leftStartPosition - nodePosition).normalized;
            Vector3 leftControlNodePosition = endPosition + left * roadWidth / 2;
            Vector3 rightControlNodePosition = endPosition - left * roadWidth / 2;

            return PopulateNodeWOIVertices(
                meshData,
                resolution,
                nodePosition,
                leftStartPosition,
                endPosition,
                leftControlNodePosition,
                rightStartPosition,
                endPosition,
                rightControlNodePosition
                );
        }

        /// <summary>
        /// Populate vertices for the start Node of a road
        /// assuming that it has no intersection.
        /// It will start on the meeting point of the road
        /// and end at the left and right side of the road.
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="roadWidth"></param>
        /// <param name="nodePosition"></param>
        /// <param name="controlPosition"></param>
        /// <param name="resolution"></param>
        public static MeshData PopulateStartNode(MeshData meshData, int roadWidth, Vector3 nodePosition, Vector3 controlPosition, int resolution) {
            Vector3 startPosition = nodePosition + (nodePosition - controlPosition).normalized * roadWidth / 2;

            Vector3 leftEndPosition = RoadUtilities.GetRoadLeftSideVertice(roadWidth, nodePosition, controlPosition);
            Vector3 rightEndPosition = RoadUtilities.GetRoadRightSideVertice(roadWidth, nodePosition, controlPosition);

            Vector3 left = (leftEndPosition - nodePosition).normalized;
            Vector3 leftControlNodePosition = startPosition + left * roadWidth / 2;
            Vector3 rightControlNodePosition = startPosition - left * roadWidth / 2;

            meshData = PopulateNodeWOIVertices(
                meshData,
                resolution,
                nodePosition,
                startPosition,
                leftEndPosition,
                leftControlNodePosition,
                startPosition,
                rightEndPosition,
                rightControlNodePosition
                );
            return meshData;
        }

        /// <summary>
        /// Builds a mesh that has the same point for 
        /// both sides of the mesh AKA a Node without
        /// intersection
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="resolution"></param>
        /// <param name="centerRoadVertice"></param>
        /// <param name="leftStartPosition"></param>
        /// <param name="leftEndPosition"></param>
        /// <param name="leftControlNodePosition"></param>
        /// <param name="rightStartPosition"></param>
        /// <param name="rightEndPosition"></param>
        /// <param name="rightControlNodePosition"></param>
        private static MeshData PopulateNodeWOIVertices(
            MeshData meshData,
            int resolution,
            Vector3 centerRoadVertice,
            Vector3 leftStartPosition,
            Vector3 leftEndPosition,
            Vector3 leftControlNodePosition,
            Vector3 rightStartPosition,
            Vector3 rightEndPosition,
            Vector3 rightControlNodePosition
        ) {
            resolution *= 3;
            float t;
            Vector3 leftRoadVertice;
            Vector3 rightRoadVertice;
            for (int i = 0; i < resolution; i++) {
                t = i / (float)(resolution - 1);
                leftRoadVertice = Bezier.QuadraticCurve(leftStartPosition, leftEndPosition, leftControlNodePosition, t);
                rightRoadVertice = Bezier.QuadraticCurve(rightStartPosition, rightEndPosition, rightControlNodePosition, t);

                meshData.AddVertice(leftRoadVertice);
                meshData.AddVertice(centerRoadVertice);
                meshData.AddVertice(rightRoadVertice);
            }
            return meshData;
        }
    }
}
