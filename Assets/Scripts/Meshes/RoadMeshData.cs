using UnityEngine;
using Roads.Utilities;
using Nodes;
using UnityEditor;
using System.Collections.Generic;
using Rafael.Utils;
using static MeshEdje;

namespace Roads.MeshHandler.Data {
    public class RoadMeshData {

        private readonly RoadObject roadObject;

        private readonly int resolution;

        /// <summary>
        /// Generates the geometry for the given Road Object
        /// </summary>
        /// <param name="roadObject"></param>
        public RoadMeshData(RoadObject roadObject) {
            this.roadObject = roadObject;
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
            Vector3 controlPosition = roadObject.ControlNodePosition - roadPosition;

            MeshEdje startCenter = startNode.GetMeshEdjeFor(roadObject, EdjePosition.StartCenter);
            MeshEdje startRight = startNode.GetMeshEdjeFor(roadObject, EdjePosition.StartRight);
            MeshEdje startLeft = startNode.GetMeshEdjeFor(roadObject, EdjePosition.StartLeft);

            MeshEdje endCenter = endNode.GetMeshEdjeFor(roadObject, EdjePosition.EndCenter);
            MeshEdje endLeft = endNode.GetMeshEdjeFor(roadObject, EdjePosition.EndLeft);
            MeshEdje endRight = endNode.GetMeshEdjeFor(roadObject, EdjePosition.EndRight);

            Vector3 startCenterPos = startCenter.Position - roadPosition;
            Vector3 startLeftPos = startLeft.Position - roadPosition;
            Vector3 startRightPos = startRight.Position - roadPosition;

            Vector3 endCenterPos = endCenter.Position - roadPosition;
            Vector3 endLeftPos = endLeft.Position - roadPosition;
            Vector3 endRightPos = endRight.Position - roadPosition;

            RafaelUtils.LineLineIntersection(
                out Vector3 controlLeftPos,
                startLeftPos,
                startLeft.Direction,
                endRightPos,
                endRight.Direction);
            RafaelUtils.LineLineIntersection(
                out Vector3 controlRightPos,
                startRightPos,
                startRight.Direction,
                endLeftPos,
                endLeft.Direction);

            meshData = MeshUtilities.PopulateRoadMeshVertices(
                meshData,
                resolution,
                startLeftPos,
                endLeftPos,
                controlLeftPos,
                startCenterPos,
                endCenterPos,
                controlPosition,
                startRightPos,
                endRightPos,
                controlRightPos);
          
            return meshData;
        }
    }
}