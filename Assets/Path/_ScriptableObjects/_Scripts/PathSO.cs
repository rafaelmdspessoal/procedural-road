using Paths.MeshHandler;
using UnityEngine;
using Path.Entities.Meshes;
using Rafael.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using World;
using Path.Entities.Pedestrian;
using Path.Entities.Vehicle;

namespace Path.Entities.SO
{
    public abstract class PathSO : ScriptableObject
    {
        public GameObject pathNodePrefab;
        public GameObject meshEdjePrefab;

        public GameObject pathObjectPrefab;
        public Material material;

        public int laneWidth;
        public int laneCount;
        public virtual int Width => laneWidth * laneCount;

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

        #region handle mesh creation
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

            Vector3 nodePosition = node.Position;
            Vector3 pathCenterPos = pathCenter.Position - nodePosition;
            Vector3 pathLeftPos = pathLeft.Position - nodePosition;
            Vector3 pathRightPos = pathRight.Position - nodePosition;

            Vector3 leftDir = pathLeftPos - pathCenterPos;
            Vector3 startPosition = pathCenterPos - pathCenterPos.normalized * leftDir.magnitude;
            Vector3 controlLeft = startPosition + leftDir;
            Vector3 controlRight = startPosition - leftDir;

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

            Vector3 nodePosition = node.Position;
            Vector3 thisPathCenterPos = thisPathCenter.Position - nodePosition;
            Vector3 thisPathLeftPos = thisPathLeft.Position - nodePosition;
            Vector3 thisPathRightPos = thisPathRight.Position - nodePosition;

            Vector3 otherPathCenterPos = otherPathCenter.Position - nodePosition;
            Vector3 otherPathLeftPos = otherPathLeft.Position - nodePosition;
            Vector3 otherPathRightPos = otherPathRight.Position - nodePosition;

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

            Vector3 nodePosition = node.Position;
            Vector3 thisPathCenterPos = thisPathCenter.Position - nodePosition;
            Vector3 thisPathLeftPos = thisPathLeft.Position - nodePosition;
            Vector3 thisPathRightPos = thisPathRight.Position - nodePosition;
            Vector3 leftPathRightPos = leftPathRight.Position - nodePosition;
            Vector3 rightPathLeftPos = rightPathLeft.Position - nodePosition;

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

            for (int i = resolution / 2; i < resolution; i++)
            {
                t = i / (float)(resolution - 1);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
                Vector3 centerPathVertice = Bezier.QuadraticCurve(startCenter, endCenter, controlCenter, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(centerPathVertice);
                meshData.AddVertice(rightPathVertice);
            }
            return meshData;
        }
        #endregion

        public virtual bool TryGetPathPositions(out Vector3 hitPosition, out GameObject hitObject)
        {
            hitObject = default;

            if (TryRaycastObject(out hitPosition, out Ground ground))
            {
                hitPosition = new Vector3(hitPosition.x, hitPosition.y + 0.1f, hitPosition.z);
                hitObject = ground.gameObject;
                return true;
            }

            return false;
        }
        public bool TryRaycastObject<T>(out Vector3 hitPosition, out T hitObject)
        {
            int radius = Width * 2;
            hitObject = default;
            hitPosition = Vector3.zero;
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity))
            {
                hitPosition = rayHit.point;
                RaycastHit[] sphereHits = Physics.SphereCastAll(hitPosition, radius, new Vector3(1f, 0, 0), radius);
                foreach (RaycastHit sphereHit in sphereHits)
                {
                    GameObject hitObj = sphereHit.transform.gameObject;

                    if (hitObj.TryGetComponent(out T obj))
                    {
                        hitObject = obj;
                        return true;
                    }
                }
            }
            return false;
        }
        public virtual bool TryConnectToSidewalk(
            out VehiclePath pathToConnect,
            out PedestrianPathNode startPathNode,
            out PedestrianPathNode endPathNode,
            out Vector3 positionToConnect)
        {
            pathToConnect = default;
            startPathNode = default;
            endPathNode = default;
            positionToConnect = Vector3.negativeInfinity;
            return false;
        }
    }
}
