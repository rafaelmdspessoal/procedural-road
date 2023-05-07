using UnityEngine;
using Roads.Utilities;
using Nodes;
using UnityEditor;
using System.Collections.Generic;

namespace Roads.MeshHandler.Data {
    public class RoadMeshData {

        private readonly RoadObject roadObject;

        private readonly int resolution;
        private readonly int roadWidth;

        /// <summary>
        /// Generates the geometry for the given Road Object
        /// </summary>
        /// <param name="roadObject"></param>
        public RoadMeshData(RoadObject roadObject) {
            this.roadObject = roadObject;

            roadWidth = roadObject.RoadWidth;
            resolution = roadObject.RoadResolution;
        }

        /// <summary>
        /// Populate mesh for given Road Object
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateRoadMeshVertices(MeshData meshData) 
        {
            Node startNode = roadObject.StartNode;
            Node endNode = roadObject.EndNode;
            Vector3 roadPosition = roadObject.transform.position;

            Vector3 startCenterNode = startNode.GetMeshEdjeFor(
                roadObject,
                MeshEdje.EdjePosition.StartCenter
                ).transform.position - roadPosition;

            Vector3 startLeft = startNode.GetMeshEdjeFor(
                roadObject,
                MeshEdje.EdjePosition.StartLeft
                ).transform.position - roadPosition;

            Vector3 startRight = startNode.GetMeshEdjeFor(
                roadObject,
                MeshEdje.EdjePosition.StartRight
                ).transform.position - roadPosition;


            Vector3 endCenterNode = endNode.GetMeshEdjeFor(
                roadObject,
                MeshEdje.EdjePosition.EndCenter
                ).transform.position - roadPosition;

            Vector3 endLeft = endNode.GetMeshEdjeFor(
                roadObject,
                MeshEdje.EdjePosition.EndLeft
                ).transform.position - roadPosition;

            Vector3 endRight = endNode.GetMeshEdjeFor(
                roadObject,
                MeshEdje.EdjePosition.EndRight
                ).transform.position - roadPosition;

            Vector3 controlPosition = roadObject.ControlNodePosition - roadPosition;

            Vector3 controlLeft;
            Vector3 controlRight;

            Vector3 n0 = (startLeft - startCenterNode).normalized;
            Vector3 n1 = (endRight - endCenterNode).normalized;

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
    }
}