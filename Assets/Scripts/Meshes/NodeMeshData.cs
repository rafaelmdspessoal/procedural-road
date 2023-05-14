using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Paths.MeshHandler;
using Rafael.Utils;
using Path.Entities;
using Path.Entities.Meshes;

namespace Nodes.MeshHandler.Data {
    public class NodeMeshData {

        private readonly NodeObject node;

        private int resolution;
        private int pathWidth;

        private Vector3 startNodePosition = Vector3.zero;

        public NodeMeshData(NodeObject node) 
        {
            this.node = node;
        }

        public MeshData PopulateMesh(MeshData meshData, PathObject connectedPath)
        {
            pathWidth = connectedPath.Width;
            resolution = connectedPath.Resolution * 2;

            if (!node.HasIntersection)
            {
                return PopulateNodeWithoutIntersection(meshData, connectedPath);
            }

            List<PathObject> adjacentPaths = node.GetAdjacentPathsTo(connectedPath).Values.ToList<PathObject>();
            if (adjacentPaths.Count == 1)
            {
                return PopulateNodeWithSingleIntersection(meshData, connectedPath, adjacentPaths.First());
            }

            return PopulateNodeWithDoubleIntersection(meshData, connectedPath, adjacentPaths);
        }

        /// <summary>
        /// Populate vertices for the start Node of a Path
        /// It stats where the left and right side of the mesh
        /// meets and finish on the left and righ sides of the
        /// Path, forming a semi circle
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateNodeWithoutIntersection(MeshData meshData, PathObject connectedPath)
        {
            MeshEdje pathCenter;
            MeshEdje pathRight;
            MeshEdje pathLeft;

            if (connectedPath.StartNode == node)
            {
                pathCenter = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.StartCenter);
                pathLeft = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.StartLeft);
                pathRight = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.StartRight);

            }
            else
            {
                pathCenter = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.EndCenter);
                pathLeft = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.EndLeft);
                pathRight = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.EndRight);
            }

            Vector3 pathCenterPos = pathCenter.Position - node.Position;
            Vector3 pathLeftPos = pathLeft.Position - node.Position;
            Vector3 pathRightPos = pathRight.Position - node.Position;

            Vector3 startPosition = pathCenterPos - pathCenterPos.normalized * pathWidth / 2;
            Vector3 leftDir = (pathLeftPos - pathCenterPos).normalized;
            Vector3 controlLeft = startPosition + leftDir * pathWidth / 2;
            Vector3 controlRight = startPosition - leftDir * pathWidth / 2;

            MeshUtilities.PopulateStartNodeVerticesWOIntersection(
                meshData,
                resolution,
                startPosition,
                pathLeftPos,
                controlLeft,
                pathCenterPos,
                pathRightPos,
                controlRight);
           
            return meshData;
        }

        /// <summary>
        /// Populate vertices for the start Node of a Path
        /// For this the left side of each path is connecte
        /// to the right side of the next one
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="adjacentPath"></param>
        /// <returns></returns>
        public MeshData PopulateNodeWithSingleIntersection(
            MeshData meshData, 
            PathObject connectedPath, 
            PathObject adjacentPath) 
        {
            MeshEdje thisPathCenter;
            MeshEdje otherPathCenter;

            MeshEdje thisPathRight;
            MeshEdje thisPathLeft;

            MeshEdje otherPathRight;
            MeshEdje otherPathLeft;

            if (connectedPath.StartNode == node)
            {
                thisPathCenter = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.StartCenter);
                thisPathLeft = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.StartLeft);
                thisPathRight = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.StartRight);

            }
            else
            {
                thisPathCenter = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.EndCenter);
                thisPathLeft = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.EndLeft);
                thisPathRight = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.EndRight);
            }

            if (adjacentPath.StartNode == node)
            {
                otherPathCenter = node.GetMeshEdjeFor(adjacentPath, MeshEdje.EdjePosition.StartCenter);
                otherPathLeft = node.GetMeshEdjeFor(adjacentPath, MeshEdje.EdjePosition.StartLeft);
                otherPathRight = node.GetMeshEdjeFor(adjacentPath, MeshEdje.EdjePosition.StartRight);

            }
            else
            {
                otherPathCenter = node.GetMeshEdjeFor(adjacentPath, MeshEdje.EdjePosition.EndCenter);
                otherPathLeft = node.GetMeshEdjeFor(adjacentPath, MeshEdje.EdjePosition.EndLeft);
                otherPathRight = node.GetMeshEdjeFor(adjacentPath, MeshEdje.EdjePosition.EndRight);
            }

            Vector3 thisPathCenterPos = thisPathCenter.Position - node.Position;
            Vector3 thisPathLeftPos = thisPathLeft.Position - node.Position;
            Vector3 thisPathRightPos = thisPathRight.Position - node.Position;

            Vector3 otherPathCenterPos = otherPathCenter.Position - node.Position;
            Vector3 otherPathLeftPos = otherPathLeft.Position - node.Position;
            Vector3 otherPathRightPos = otherPathRight.Position - node.Position;

            RafaelUtils.LineLineIntersection(out Vector3 controlLeftPos,
                thisPathRightPos,
                thisPathRight.Direction,
                otherPathLeftPos,
                otherPathLeft.Direction);
            RafaelUtils.LineLineIntersection(
                out Vector3 controlRightPos,
                thisPathLeftPos,
                thisPathLeft.Direction,
                otherPathRightPos,
                otherPathRight.Direction);


            meshData = MeshUtilities.PopulateNodeMeshVerticesWSIntersection(
                meshData,
                resolution,
                thisPathRightPos,
                otherPathLeftPos,
                controlLeftPos,
                thisPathCenterPos,
                otherPathCenterPos,
                startNodePosition,
                thisPathLeftPos,
                otherPathRightPos,
                controlRightPos);

            return meshData;
        }

        /// <summary>
        /// Populate vertices for the start Node of a Path
        /// For this the left side of the path connects to
        /// the right side of the left path and the right
        /// side connects to the left side of the right path
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="adjacentPaths"></param>
        /// <returns></returns>
        public MeshData PopulateNodeWithDoubleIntersection(
            MeshData meshData, 
            PathObject connectedPath, 
            List<PathObject> adjacentPaths)
        {
            if (adjacentPaths.Count != 2)
            {
                throw new System.Exception("Intersection MUST have two paths, but has " + adjacentPaths.Count);
            }

            PathObject leftPath = adjacentPaths.First();
            PathObject rightPath = adjacentPaths.Last();

            MeshEdje thisPathCenter;
            MeshEdje thisPathRight;
            MeshEdje thisPathLeft;

            MeshEdje leftPathRight;
            MeshEdje rightPathLeft;

            if (connectedPath.StartNode == node)
            {
                thisPathCenter = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.StartCenter);
                thisPathLeft = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.StartLeft);
                thisPathRight = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.StartRight);
            }
            else
            {
                thisPathCenter = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.EndCenter);
                thisPathLeft = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.EndLeft);
                thisPathRight = node.GetMeshEdjeFor(connectedPath, MeshEdje.EdjePosition.EndRight);
            }

            if (leftPath.StartNode == node)
                leftPathRight = node.GetMeshEdjeFor(leftPath, MeshEdje.EdjePosition.StartRight);
            else
                leftPathRight = node.GetMeshEdjeFor(leftPath, MeshEdje.EdjePosition.EndRight);

            if (rightPath.StartNode == node)
                rightPathLeft = node.GetMeshEdjeFor(rightPath, MeshEdje.EdjePosition.StartLeft);
            else
                rightPathLeft = node.GetMeshEdjeFor(rightPath, MeshEdje.EdjePosition.EndLeft);

            Vector3 thisPathCenterPos = thisPathCenter.Position - node.Position;
            Vector3 thisPathLeftPos = thisPathLeft.Position - node.Position;
            Vector3 thisPathRightPos = thisPathRight.Position - node.Position;
            Vector3 leftPathRightPos = leftPathRight.Position - node.Position;
            Vector3 rightPathLeftPos = rightPathLeft.Position - node.Position;

            Vector3 intersectionCenter = -thisPathCenterPos;

            RafaelUtils.LineLineIntersection(
                out Vector3 controlLeftPos,
                leftPathRightPos,
                leftPathRight.Direction,
                thisPathLeftPos,
                thisPathLeft.Direction);
            RafaelUtils.LineLineIntersection(
                out Vector3 controlRightPos,
                rightPathLeftPos,
                rightPathLeft.Direction,
                thisPathRightPos,
                thisPathRight.Direction);

            meshData = MeshUtilities.PopulateNodeMeshVerticesWDIntersection(
                meshData,
                resolution,

                leftPathRightPos,
                thisPathLeftPos,
                controlLeftPos,

                intersectionCenter,
                thisPathCenterPos,

                rightPathLeftPos,
                thisPathRightPos,
                controlRightPos);

            return meshData;
        }
    }
}
