using UnityEngine;
using Rafael.Utils;
using Path.Entities;
using Path.Entities.Meshes;

namespace Paths.MeshHandler.Data {
    public class PathMeshData {

        private readonly PathObject pathObject;

        private readonly int resolution;

        /// <summary>
        /// Generates the geometry for the given Path Object
        /// </summary>
        /// <param name="pathObject"></param>
        public PathMeshData(PathObject pathObject) {
            this.pathObject = pathObject;
            resolution = pathObject.Resolution;
        }

        /// <summary>
        /// Populate mesh for given Path Object
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulatePathMeshVertices(MeshData meshData)
        {
            NodeObject startNode = pathObject.StartNode;
            NodeObject endNode = pathObject.EndNode;
            Vector3 pathPosition = pathObject.transform.position;
            Vector3 controlPosition = pathObject.ControlPosition - pathPosition;

            MeshEdje startCenter = startNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartCenter);
            MeshEdje startRight = startNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartRight);
            MeshEdje startLeft = startNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartLeft);

            MeshEdje endCenter = endNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndCenter);
            MeshEdje endLeft = endNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndLeft);
            MeshEdje endRight = endNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndRight);

            Vector3 startCenterPos = startCenter.Position - pathPosition;
            Vector3 startLeftPos = startLeft.Position - pathPosition;
            Vector3 startRightPos = startRight.Position - pathPosition;

            Vector3 endCenterPos = endCenter.Position - pathPosition;
            Vector3 endLeftPos = endLeft.Position - pathPosition;
            Vector3 endRightPos = endRight.Position - pathPosition;

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

            meshData = MeshUtilities.PopulatePathMeshVertices(
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