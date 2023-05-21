using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Path.Entities.Pedestrian;
using Path.Entities.Meshes;
using Path.Utilities;
using Path.Entities.SO;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Path.Entities
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    public abstract class NodeObject : MonoBehaviour, IEquatable<NodeObject>
    {
        public enum PathFor
        {
            Pedestrian,
            Vehicle,
        }

        [SerializeField] private GameObject pathNodePrefab;
        [SerializeField] private GameObject meshEdjePrefab;

        [SerializeField] private List<PathObject> connectedPathList = new();

        private readonly Dictionary<PathObject, List<PathNodeObject>> pathNodesDict = new();
        private readonly Dictionary<PathObject, List<MeshEdje>> meshEdjesDict = new();

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        [SerializeField] protected PathFor pathFor;
        public PathFor PathEntity => pathFor;

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
            List<PathNodeObject> pathNodesList = GetAllPathNodes();

            foreach (PathNodeObject endPathNode in pathNodesList)
            {
                if (endPathNode.IsStartOfPath) continue;
                foreach (PathNodeObject startPathNode in pathNodesList)
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
                transform).GetComponent<MeshEdje>();

            if (isStartNode)
                edjePosition = MeshEdje.EdjePosition.StartCenter;
            else
                edjePosition = MeshEdje.EdjePosition.EndCenter;
            centerEdje.Init(edjePosition);

            MeshEdje leftEdje = Instantiate(
                meshEdjePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<MeshEdje>();

            if (isStartNode)
                edjePosition = MeshEdje.EdjePosition.StartLeft;
            else
                edjePosition = MeshEdje.EdjePosition.EndLeft;

            leftEdje.Init(edjePosition);

            MeshEdje rightEdje = Instantiate(
                meshEdjePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<MeshEdje>();

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
            PathNodeObject newEndPathNode = Instantiate(
                pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PathNodeObject>();

            PathNodeObject newStartPathNode = Instantiate(
                pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PathNodeObject>();

            if (IsStartNodeOf(pathObject))
            {
                newStartPathNode.Init(PathNodeObject.OnPathPosition.StartNodeStartPath);
                newEndPathNode.Init(PathNodeObject.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                newStartPathNode.Init(PathNodeObject.OnPathPosition.EndNodeStartPath);
                newEndPathNode.Init(PathNodeObject.OnPathPosition.EndNodeEndPath);
            }
            pathNodesDict.Add(pathObject, new List<PathNodeObject> { newStartPathNode, newEndPathNode });
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
        public void SetMesh(Mesh mesh)
        {
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            // Material tiling will depend on the path lengh, so let's have
            // different instances
            meshRenderer.material = new Material(connectedPathList[0].PathSO.material);

            if (connectedPathList.Count == 2)
            {
                Vector3 center0;
                Vector3 center1;

                if (IsStartNodeOf(connectedPathList[0]))
                    center0 = GetMeshEdjeFor(connectedPathList[0], MeshEdje.EdjePosition.StartCenter).Position;
                else
                    center0 = GetMeshEdjeFor(connectedPathList[0], MeshEdje.EdjePosition.EndCenter).Position;
                
                if (IsStartNodeOf(connectedPathList[1]))
                    center1 = GetMeshEdjeFor(connectedPathList[1], MeshEdje.EdjePosition.StartCenter).Position;
                else
                    center1 = GetMeshEdjeFor(connectedPathList[1], MeshEdje.EdjePosition.EndCenter).Position;


                float pathLengh = Bezier.GetLengh(center0, center1, Position);
                int textureRepead = Mathf.RoundToInt(connectedPathList[0].PathSO.textureTiling * pathLengh * .01f);
                meshRenderer.material.mainTextureScale = new Vector2(.5f, textureRepead);
                meshRenderer.material.mainTextureOffset = new Vector2(0, 0);
            }
            else
            {
                meshRenderer.material.mainTextureScale = new Vector2(.5f, 1);
                meshRenderer.material.mainTextureOffset = new Vector2(0, 0);
            }
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
            foreach (PathNodeObject pathNode in pathNodesDict[pathObject])
            {
                Destroy(pathNode.gameObject);
            }
            pathNodesDict.Remove(pathObject);
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

            PathNodeObject startPathNode;
            PathNodeObject endPathNode;

            if (IsStartNodeOf(pathObject))
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartCenter);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartLeft);
                right = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartRight);

                startPathNode = GetPathNodeFor(pathObject, PathNodeObject.OnPathPosition.StartNodeStartPath);
                endPathNode = GetPathNodeFor(pathObject, PathNodeObject.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndCenter);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndLeft);
                right = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndRight);

                startPathNode = GetPathNodeFor(pathObject, PathNodeObject.OnPathPosition.EndNodeStartPath);
                endPathNode = GetPathNodeFor(pathObject, PathNodeObject.OnPathPosition.EndNodeEndPath);
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
        public float GetNodeSizeFor(PathObject pathObject)
        {
            if (!HasIntersection) return 0;

            Dictionary<float, PathObject> adjacentPaths = GetAdjacentPathsTo(pathObject);
            float offset;
            float cosAngle;
            int width = GetMaxWidthIn(adjacentPaths.Values.ToList()) / 2;
            if (adjacentPaths.Count == 1)
            {
                float angle = adjacentPaths.First().Key;
                if (angle > 180) angle = Mathf.Abs(angle - 360);
                angle = Mathf.Clamp(angle, 0, 90);
                angle *= Mathf.Deg2Rad;
                cosAngle = Mathf.Cos(angle - Mathf.PI / 2);
                offset = (1 + Mathf.Cos(angle)) * (width * 1.2f) / cosAngle;
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
            offset = (1 + Mathf.Cos(smallestAngle)) * (width * 1.2f) / cosAngle;

            return offset;
        }
        private int GetMaxWidthIn(List<PathObject> pathObjects)
        {
            int width = 0;
            foreach (PathObject pathObject in pathObjects)
            {
                if (pathObject.Width > width) width = pathObject.Width;
            }
            return width;
        }
        public Vector3 Direction => Position - connectedPathList.First().ControlPosition;
        public Vector3 Position => transform.position;
        public List<PathObject> ConnectedPaths => connectedPathList;
        public List<NodeObject> GetConnectedNodes()
        {
            List<NodeObject> connectedNodes = new();
            foreach (PathObject connectedPath in connectedPathList)
            {
                connectedNodes.Add(connectedPath.OtherNodeTo(this));
            }

            return connectedNodes;
        }
        public List<PathNodeObject> GetAllPathNodes()
        {
            List<PathNodeObject> pathNodesList = new();

            foreach (List<PathNodeObject> pathNodes in pathNodesDict.Values)
            {
                pathNodesList.AddRange(pathNodes);
            }
            return pathNodesList;
        }
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
        public bool Equals(NodeObject other)
        {
            return Vector3.SqrMagnitude(Position - other.Position) < 0.0001f;
        }
        public bool IsStartNodeOf(PathObject pathObject)
        {
            return pathObject.StartNode.Equals(this);
        }
        public bool HasConnectedPaths => connectedPathList.Count > 0;
        public bool HasIntersection => connectedPathList.Count > 1;
        public MeshEdje GetMeshEdjeFor(PathObject pathObject, MeshEdje.EdjePosition edjePosition)
        {
            List<MeshEdje> meshEdjes = meshEdjesDict[pathObject];
            return meshEdjes.Find(x => x.EdjePos == edjePosition);
        }
        public PathNodeObject GetPathNodeFor(PathObject pathObject, PathNodeObject.OnPathPosition pathPosition)
        {
            List<PathNodeObject> pathNodes = pathNodesDict[pathObject];
            return pathNodes.Find(x => x.PathPosition == pathPosition);
        }

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