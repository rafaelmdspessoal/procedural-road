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

            return MeshUtilities.PopulateNodeWOIVertices(
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

            meshData = MeshUtilities.PopulateNodeWOIVertices(
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

    }
}
