using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Path.Entities.Pedestrian;
using Path.Entities.Meshes;
using Path.Utilities;

namespace Path.Entities
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    public class NodeObject : MonoBehaviour, IEquatable<NodeObject>
    {
        [SerializeField] private GameObject pathNodePrefab;
        [SerializeField] private GameObject meshEdjePrefab;

        [SerializeField] private List<PathObject> connectedPathList = new();

        private readonly Dictionary<PathObject, List<PedestrianPathNode>> pathNodesDict = new();
        private readonly Dictionary<PathObject, List<MeshEdje>> meshEdjesDict = new();

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
        }

        public void AddPath(PathObject pathObject)
        {
            if (!connectedPathList.Contains(pathObject))
            {
                connectedPathList.Add(pathObject);
                CreatePathNodeFor(pathObject);
                CreateMeshEdjesFor(pathObject);
                foreach (PathObject connectedPath in GetAdjacentPathsTo(pathObject).Values)
                {
                    UpdateEdjePositions(connectedPath);
                    UpdatePathPostions(connectedPath);
                    ConnectPathNodes();
                }
                UpdateEdjePositions(pathObject);
                UpdatePathPostions(pathObject);
                ConnectPathNodes();
                pathObject.OnPathRemoved += PathObject_OnPathRemoved;
            }
        }

        public void ConnectPathNodes()
        {
            List<PedestrianPathNode> pathNodesList = GetAllPathNodes();

            foreach (PedestrianPathNode endPathNode in pathNodesList)
            {
                if (endPathNode.IsStartOfPath) continue;
                foreach (PedestrianPathNode startPathNode in pathNodesList)
                {
                    if (!startPathNode.IsStartOfPath) continue;
                    endPathNode.AddPathNode(startPathNode);
                }
            }
        }
        private void CreateMeshEdjesFor(PathObject pathObject)
        {
            MeshEdje.EdjePosition edjePosition;
            bool isStartNode = IsStartNodeOf(pathObject);
            MeshEdje centerEdje = Instantiate(
                meshEdjePrefab,
                transform.position,
                Quaternion.identity,
                transform
                ).GetComponent<MeshEdje>();

            if (isStartNode)
                edjePosition = MeshEdje.EdjePosition.StartCenter;
            else
                edjePosition = MeshEdje.EdjePosition.EndCenter;
            centerEdje.Init(edjePosition);

            MeshEdje leftEdje = Instantiate(
                meshEdjePrefab,
                transform.position,
                Quaternion.identity,
                transform
                ).GetComponent<MeshEdje>();

            if (isStartNode)
                edjePosition = MeshEdje.EdjePosition.StartLeft;
            else
                edjePosition = MeshEdje.EdjePosition.EndLeft;

            leftEdje.Init(edjePosition);

            MeshEdje rightEdje = Instantiate(
                meshEdjePrefab,
                transform.position,
                Quaternion.identity,
                transform
                ).GetComponent<MeshEdje>();

            if (isStartNode)
                edjePosition = MeshEdje.EdjePosition.StartRight;
            else
                edjePosition = MeshEdje.EdjePosition.EndRight;

            rightEdje.Init(edjePosition);

            if (meshEdjesDict.ContainsKey(pathObject))
                meshEdjesDict[pathObject].AddRange(new List<MeshEdje> { centerEdje, leftEdje, rightEdje });
            else
                meshEdjesDict.Add(pathObject, new List<MeshEdje> { centerEdje, leftEdje, rightEdje });
        }
        private void CreatePathNodeFor(PathObject pathObject)
        {
            PedestrianPathNode newEndPathNode = Instantiate(
                pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PedestrianPathNode>();

            PedestrianPathNode newStartPathNode = Instantiate(
                pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PedestrianPathNode>();

            if (IsStartNodeOf(pathObject))
            {
                newStartPathNode.Init(PedestrianPathNode.OnPathPosition.StartNodeStartPath);
                newEndPathNode.Init(PedestrianPathNode.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                newStartPathNode.Init(PedestrianPathNode.OnPathPosition.EndNodeStartPath);
                newEndPathNode.Init(PedestrianPathNode.OnPathPosition.EndNodeEndPath);
            }
            pathNodesDict.Add(pathObject, new List<PedestrianPathNode> { newStartPathNode, newEndPathNode });
        }
        public bool Equals(NodeObject other)
        {
            return Vector3.SqrMagnitude(Position - other.Position) < 0.0001f;
        }
        public List<PedestrianPathNode> GetAllPathNodes()
        {
            List<PedestrianPathNode> pathNodesList = new();

            foreach (List<PedestrianPathNode> pathNodes in pathNodesDict.Values)
            {
                pathNodesList.AddRange(pathNodes);
            }
            return pathNodesList;
        }
        public List<NodeObject> GetConnectedNodes()
        {
            List<NodeObject> connectedNodes = new();
            foreach (PathObject connectedPath in connectedPathList)
            {
                connectedNodes.Add(connectedPath.OtherNodeTo(this));
            }

            return connectedNodes;
        }
        public MeshEdje GetMeshEdjeFor(PathObject pathObject, MeshEdje.EdjePosition edjePosition)
        {
            List<MeshEdje> meshEdjes = meshEdjesDict[pathObject];
            return meshEdjes.Find(x => x.EdjePos == edjePosition);
        }
        public float GetNodeSizeFor(PathObject pathObject)
        {
            if (!HasIntersection) return 0;

            Dictionary<float, PathObject> adjacentPaths = GetAdjacentPathsTo(pathObject);
            float offset;
            float cosAngle;
            int width = pathObject.Width / 2;
            if (adjacentPaths.Count == 1)
            {
                float angle = adjacentPaths.First().Key;
                if (angle > 180) angle = Mathf.Abs(angle - 360);
                angle = Mathf.Clamp(angle, 0, 90);
                angle *= Mathf.Deg2Rad;
                cosAngle = Mathf.Cos(angle - Mathf.PI / 2);
                offset = (1 + Mathf.Cos(angle)) * (width + 0.15f) / cosAngle;
                return offset;
            }

            float leftAngle = adjacentPaths.First().Key;
            float rightAngle = adjacentPaths.Last().Key;
            float smallestAngle;

            if (rightAngle > 180) rightAngle = Mathf.Abs(rightAngle - 360);
            smallestAngle = Mathf.Min(leftAngle, rightAngle);
            smallestAngle = Mathf.Clamp(smallestAngle, 0, 90);
            smallestAngle *= Mathf.Deg2Rad;
            cosAngle = Mathf.Cos(smallestAngle - Mathf.PI / 2);
            offset = (1 + Mathf.Cos(smallestAngle)) * (width + 0.15f) / cosAngle;

            return offset;
        }
        public PedestrianPathNode GetPathNodeFor(PathObject pathObject, PedestrianPathNode.OnPathPosition pathPosition)
        {
            List<PedestrianPathNode> pathNodes = pathNodesDict[pathObject];
            return pathNodes.Find(x => x.PathPosition == pathPosition);
        }
        public bool IsStartNodeOf(PathObject pathObject)
        {
            return pathObject.StartNode.Equals(this);
        }


        private void PathObject_OnPathRemoved(object sender, EventArgs e)
        {
            PathObject pathObject = (PathObject)sender;
            List<PathObject> adjacentPaths = GetAdjacentPathsTo(pathObject).Values.ToList();
            if (connectedPathList.Contains(pathObject))
            {
                connectedPathList.Remove(pathObject);
                RemoveMeshEdjesFor(pathObject);
                RemovePedestrianPathNodesFor(pathObject);
            }

            foreach (PathObject pathToUpdate in adjacentPaths)
            {
                UpdateEdjePositions(pathToUpdate);
                UpdatePathPostions(pathToUpdate);
                pathToUpdate.UpdateMesh();
            }

            if (HasConnectedPaths)
            {
                ConnectPathNodes();
                Mesh nodeMesh = pathObject.PathSO.CreateNodeMesh(this);
                SetMesh(nodeMesh);
            }
            else
            {
                PathManager.Instance.RemoveNode(this);
            }
        }
        public void UpdateEdjePositions(PathObject pathObject)
        {
            float meshStartOffset = GetNodeSizeFor(pathObject);
            int pathWidth = pathObject.Width;
            Vector3 controlPosition = pathObject.ControlPosition;
            Vector3 leftMeshPosition;
            Vector3 rightMeshPosition;
            Vector3 centerMeshPosition;
            Vector3 direction;

            MeshEdje center;
            MeshEdje left;
            MeshEdje right;

            int flipDirection = 1;
            if (IsStartNodeOf(pathObject))
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartCenter);
                right = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartLeft);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartRight);
                flipDirection = -1;
            }
            else
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndCenter);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndLeft);
                right = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndRight);
            }

            centerMeshPosition = Bezier.GetOffsettedPosition(
                Position,
                pathObject.OtherNodeTo(this).Position,
                controlPosition,
                meshStartOffset);

            direction = Bezier.GetTangentAt(pathObject, centerMeshPosition, out _, out _);

            leftMeshPosition = PathUtilities.GetLeftPointTo(centerMeshPosition, direction, pathWidth / 2);
            rightMeshPosition = PathUtilities.GetRightPointTo(centerMeshPosition, direction, pathWidth / 2);

            center.transform.position = centerMeshPosition;
            left.transform.position = leftMeshPosition;
            right.transform.position = rightMeshPosition;

            center.transform.rotation = Quaternion.LookRotation(direction * flipDirection);
            left.transform.rotation = Quaternion.LookRotation(direction * flipDirection);
            right.transform.rotation = Quaternion.LookRotation(direction * flipDirection);
        }
        public void UpdatePathPostions(PathObject pathObject)
        {
            MeshEdje center;
            MeshEdje left;
            MeshEdje right;

            PedestrianPathNode startPathNode;
            PedestrianPathNode endPathNode;

            if (IsStartNodeOf(pathObject))
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartCenter);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartLeft);
                right = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartRight);

                startPathNode = GetPathNodeFor(pathObject, PedestrianPathNode.OnPathPosition.StartNodeStartPath);
                endPathNode = GetPathNodeFor(pathObject, PedestrianPathNode.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndCenter);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndLeft);
                right = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndRight);

                startPathNode = GetPathNodeFor(pathObject, PedestrianPathNode.OnPathPosition.EndNodeStartPath);
                endPathNode = GetPathNodeFor(pathObject, PedestrianPathNode.OnPathPosition.EndNodeEndPath);
            }

            Vector3 centerPos = center.Position; 
            Vector3 leftPos = left.Position;
            Vector3 rightPos = right.Position;

            Vector3 startPathPosition = (centerPos + leftPos) / 2f;
            Vector3 endPathPosition = (centerPos + rightPos) / 2f;

            startPathNode.transform.position = startPathPosition;
            endPathNode.transform.position = endPathPosition;

            startPathNode.transform.rotation = Quaternion.LookRotation(center.Direction);
            endPathNode.transform.rotation = Quaternion.LookRotation(center.Direction);
        }
        public void SetMesh(Mesh mesh)
        {
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            // Material tiling will depend on the path lengh, so let's have
            // different instances
            meshRenderer.material = new Material(connectedPathList[0].PathSO.material);

            meshRenderer.material.mainTextureScale = new Vector2(.5f, 1);
            meshRenderer.material.mainTextureOffset = new Vector2(0, 0);
        }
        private void RemoveMeshEdjesFor(PathObject pathObject)
        {
            foreach (MeshEdje meshEdje in meshEdjesDict[pathObject])
            {
                Destroy(meshEdje.gameObject);
            }
            meshEdjesDict.Remove(pathObject);
        }
 
        private void RemovePedestrianPathNodesFor(PathObject pathObject)
        {
            foreach (PedestrianPathNode pathNode in pathNodesDict[pathObject])
            {
                Destroy(pathNode.gameObject);
            }
            pathNodesDict.Remove(pathObject);
        }

        public List<PathObject> ConnectedPaths => connectedPathList;
        public Vector3 Direction => Position - connectedPathList.First().ControlPosition;
        public Dictionary<float, PathObject> GetAdjacentPathsTo(PathObject pathObject)
        {
            Dictionary<float, PathObject> connectedPathsDict = new();

            if (!HasIntersection) return connectedPathsDict;

            if (HasIntersection && pathObject != null)
            {
                Vector3 pathObjectDirection = Position - pathObject.ControlPosition;

                foreach (PathObject connectedPath in connectedPathList)
                {
                    if (connectedPath != pathObject)
                    {
                        Vector3 connectedPathDirection = Position - connectedPath.ControlPosition;
                        float angle = Vector3.SignedAngle(pathObjectDirection, connectedPathDirection, transform.up);
                        if (angle < 0) angle += 360;
                        if (!connectedPathsDict.ContainsKey(angle))
                            connectedPathsDict.Add(angle, connectedPath);
                    }
                }
            }

            connectedPathsDict = connectedPathsDict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            Dictionary<float, PathObject> adjacentPaths = new()
            {
                { connectedPathsDict.First().Key, connectedPathsDict.First().Value }
            };

            if (connectedPathsDict.Count > 1)
                adjacentPaths.Add(connectedPathsDict.Last().Key, connectedPathsDict.Last().Value);

            return adjacentPaths;
        }
        public bool HasConnectedPaths => connectedPathList.Count > 0;
        public bool HasIntersection => connectedPathList.Count > 1;
        public Vector3 Position => transform.position;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var items in meshEdjesDict.Values)
            {
                foreach (var item in items)
                {
                    Gizmos.DrawLine(item.Position, item.Position + item.Direction);
                }
            }

            Gizmos.color = Color.blue;
            foreach (var items in pathNodesDict.Values)
            {
                foreach (var item in items)
                {
                    Gizmos.DrawLine(item.Position, item.Position + item.Direction);
                }
            }
        }
    }
}