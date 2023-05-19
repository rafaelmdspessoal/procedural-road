using Paths.MeshHandler;
using UnityEngine;
using Path.Entities.Meshes;
using Rafael.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Path.Entities.SO
{
    public class PathSO : ScriptableObject
    {
        public GameObject pathObjectPrefab;
        public Material material;

        public int width;
        public int resolution;
        public int textureTiling;

        public int minIntersectionAngle;

        public PathObject CreatePathObject(
            NodeObject startNode,
            NodeObject endNode,
            Vector3 controlPosition,
            Transform parentTransform)
        {
            Vector3 pathPosition = (startNode.Position + endNode.Position) / 2;
            PathObject pathObject = Instantiate(
                pathObjectPrefab,
                pathPosition,
                Quaternion.identity,
                parentTransform).GetComponent<PathObject>();

            pathObject.PlacePath(startNode, endNode, controlPosition);
            return pathObject;
        }

        public void SplitPathObject(
            NodeObject startNode,
            NodeObject endNode,
            NodeObject intersectionNode,
            Vector3 startControlPosition,
            Vector3 endControlPosition,
            Transform parentTransform)
        {
            CreatePathObject(startNode, intersectionNode, startControlPosition, parentTransform);
            CreatePathObject(intersectionNode, endNode, endControlPosition, parentTransform);
        }

        public Mesh CreatePathMesh(PathObject pathObject)
        {
            MeshData meshData = new();
            PopulatePathMesh(meshData, pathObject);

            MeshUtilities.PopulateMeshTriangles(meshData);
            MeshUtilities.PopulateMeshUvs(meshData);

            Mesh mesh = MeshUtilities.LoadMesh(meshData);
            return mesh;
        }
        public Mesh CreateNodeMesh(NodeObject node)
        {
            CombineInstance[] meshes = new CombineInstance[node.ConnectedPaths.Count];

            int i = 0;
            foreach (PathObject connectedPath in node.ConnectedPaths)
            {
                MeshData meshData = new();
                PopulateNodeMesh(meshData, connectedPath, node);

                MeshUtilities.PopulateMeshTriangles(meshData);
                MeshUtilities.PopulateMeshUvs(meshData);
                meshes[i].mesh = MeshUtilities.LoadMesh(meshData);
                i++;
            }

            Mesh mesh = new();
            mesh.CombineMeshes(meshes, true, false);
            return mesh;
        }
        
        private MeshData PopulatePathMesh(MeshData meshData, PathObject pathObject)
        {
            NodeObject startNode = pathObject.StartNode;
            NodeObject endNode = pathObject.EndNode;

            MeshEdje startCenter = startNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartCenter);
            MeshEdje startRight = startNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartRight);
            MeshEdje startLeft = startNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartLeft);

            MeshEdje endCenter = endNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndCenter);
            MeshEdje endLeft = endNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndLeft);
            MeshEdje endRight = endNode.GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndRight);

            Vector3 pathPosition = pathObject.transform.position;
            pathPosition.y = 0;

            Vector3 startCenterPos = startCenter.Position - pathPosition;
            Vector3 startLeftPos = startLeft.Position - pathPosition;
            Vector3 startRightPos = startRight.Position - pathPosition;

            Vector3 endCenterPos = endCenter.Position - pathPosition;
            Vector3 endLeftPos = endLeft.Position - pathPosition;
            Vector3 endRightPos = endRight.Position - pathPosition;

            Vector3 controlPosition = pathObject.ControlPosition - pathPosition;

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

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startLeftPos, endRightPos, controlLeftPos, t);
                Vector3 centerPathVertice = Bezier.QuadraticCurve(startCenterPos, endCenterPos, controlPosition, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startRightPos, endLeftPos, controlRightPos, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(centerPathVertice);
                meshData.AddVertice(rightPathVertice);
            }

            return meshData;
        }
        private MeshData PopulateNodeMesh(MeshData meshData, PathObject connectedPath, NodeObject node)
        {
            if (!node.HasIntersection)
                return PopulateNodeWithoutIntersection(meshData, connectedPath, node);

            List<PathObject> adjacentPaths = node.GetAdjacentPathsTo(connectedPath).Values.ToList();
            if (adjacentPaths.Count == 1)
                return PopulateNodeWithSingleIntersection(meshData, connectedPath, adjacentPaths.First(), node);

            return PopulateNodeWithDoubleIntersection(meshData, connectedPath, adjacentPaths, node);
        }
        private MeshData PopulateNodeWithoutIntersection(MeshData meshData, PathObject connectedPath, NodeObject node)
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

            Vector3 leftDir = (pathLeftPos - pathCenterPos).normalized;
            Vector3 startPosition = pathCenterPos - pathCenterPos.normalized * width / 2;
            Vector3 controlLeft = startPosition + leftDir * width / 2;
            Vector3 controlRight = startPosition - leftDir * width / 2;

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startPosition, pathLeftPos, controlLeft, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startPosition, pathRightPos, controlRight, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(pathCenterPos);
                meshData.AddVertice(rightPathVertice);
            }
            return meshData;
        }
        private MeshData PopulateNodeWithSingleIntersection(
            MeshData meshData,
            PathObject connectedPath,
            PathObject adjacentPath,
            NodeObject node)
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


            meshData = PopulateNodeMeshDataWithIntersection(
                meshData,
                thisPathRightPos,
                otherPathLeftPos,
                controlLeftPos,
                thisPathCenterPos,
                otherPathCenterPos,
                Vector3.zero,
                thisPathLeftPos,
                otherPathRightPos,
                controlRightPos);

            return meshData;
        }

        private MeshData PopulateNodeWithDoubleIntersection(
            MeshData meshData,
            PathObject connectedPath,
            List<PathObject> adjacentPaths,
            NodeObject node)
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

            meshData = PopulateNodeMeshDataWithIntersection(
                meshData,

                leftPathRightPos,
                thisPathLeftPos,
                controlLeftPos,

                intersectionCenter,
                thisPathCenterPos,
                (intersectionCenter + thisPathCenterPos) / 2,

                rightPathLeftPos,
                thisPathRightPos,
                controlRightPos);

            return meshData;
        }
        private MeshData PopulateNodeMeshDataWithIntersection(
            MeshData meshData,
            Vector3 startLeft,
            Vector3 endLeft,
            Vector3 controlLeft,
            Vector3 startCenter,
            Vector3 endCenter,
            Vector3 controlCenter,
            Vector3 startRight,
            Vector3 endRight,
            Vector3 controlRight)
        {
            float t;

            for (int i = resolution / 2 - 1; i < resolution - 1; i++)
            {
                t = i / (float)(resolution - 2);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
                Vector3 centerPathVertice = Bezier.QuadraticCurve(startCenter, endCenter, controlCenter, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(centerPathVertice);
                meshData.AddVertice(rightPathVertice);
            }
            return meshData;
        }
    }
}
