using Roads;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Nodes.MeshHandler;
using System;
using UnityEditor.Experimental.GraphView;
using Roads.Utilities;
using static MeshEdje;

namespace Nodes {
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    public class Node : MonoBehaviour, IEquatable<Node>
    {
        [SerializeField] private GameObject pathNodePrefab;
        [SerializeField] private GameObject meshEdjePrefab;

        [SerializeField] private List<RoadObject> connectedRoadsList = new();

        private List<PathNode> startPathNodeList = new();
        private List<PathNode> endPathNodeList = new();

        private readonly Dictionary<RoadObject, List<PathNode>> connectedPathNodesDict = new();
        private readonly Dictionary<RoadObject, List<MeshEdje>> meshEdjesDict = new();

        private readonly Dictionary<Vector3, PathNode> startPathNodeDict = new();
        private readonly Dictionary<Vector3, PathNode> endPathNodeDict = new();

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;


        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
        }

        public void Init(RoadObject roadObject)
        {
            UpdateEdjePositions(roadObject);
        }

        private void UpdateEdjePositions(RoadObject roadObject)
        {
            float meshStartOffset = GetNodeSizeForRoad(roadObject);
            float roadWidth = roadObject.RoadWidth;
            Vector3 controlPosition = roadObject.ControlNodePosition;

            Vector3 centerMeshPosition = Bezier.GetOffsettedPosition(
                Position,
                roadObject.OtherNodeTo(this).Position,
                controlPosition,
                meshStartOffset);

            Vector3 leftMeshPosition = RoadUtilities.GetRoadLeftSideVertice(
                roadWidth,
                centerMeshPosition,
                controlPosition);
            Vector3 rightMeshPosition = RoadUtilities.GetRoadRightSideVertice(
               roadWidth,
               centerMeshPosition,
               controlPosition);

            if (IsStartNodeOf(roadObject))
            {
                GetMeshEdjeFor(roadObject, EdjePosition.StartCenter).transform.position = centerMeshPosition;
                GetMeshEdjeFor(roadObject, EdjePosition.StartLeft).transform.position = leftMeshPosition;
                GetMeshEdjeFor(roadObject, EdjePosition.StartRight).transform.position = rightMeshPosition;
            }
            else
            {
                GetMeshEdjeFor(roadObject, EdjePosition.EndCenter).transform.position = centerMeshPosition;
                GetMeshEdjeFor(roadObject, EdjePosition.EndLeft).transform.position = leftMeshPosition;
                GetMeshEdjeFor(roadObject, EdjePosition.EndRight).transform.position = rightMeshPosition;
            }
        }

        public MeshEdje GetMeshEdjeFor(RoadObject roadObject, EdjePosition edjePosition)
        {
            List<MeshEdje> meshEdjes = meshEdjesDict[roadObject];
            return meshEdjes.Find(x => x.EdjePos == edjePosition);
        }

        public void SetPathPostions()
        {
            List<Vector3> roadsStartMeshPositions = new();
            startPathNodeList.Clear();
            endPathNodeList.Clear();

            startPathNodeDict.Clear();
            endPathNodeDict.Clear();

            var pathNodes = transform.GetComponentsInChildren<PathNode>();
            foreach (var pathNode in pathNodes) {
                pathNode.ClearConnections();
                Destroy(pathNode.gameObject);
            }
            foreach (Vector3 roadCenterMeshPosition in roadsStartMeshPositions)
            {
                Vector3 dir = roadCenterMeshPosition - Position;
                Vector3 left = new Vector3(-dir.z, dir.y, dir.x);
                Vector3 startPathPosition = roadCenterMeshPosition - left.normalized;
                Vector3 endPathPosition = roadCenterMeshPosition + left.normalized;

                GameObject newStartPathNodeObj = Instantiate(pathNodePrefab, startPathPosition, Quaternion.identity, this.transform);

                PathNode newStartPathNode = newStartPathNodeObj.GetComponent<PathNode>();
                newStartPathNode.Init(PathNode.PathOrientation.Start, "Start path Node");

                GameObject newEndPathNodeObj = Instantiate(pathNodePrefab, endPathPosition, Quaternion.identity, this.transform);

                PathNode newEndPathNode = newEndPathNodeObj.GetComponent<PathNode>();
                newEndPathNode.Init(PathNode.PathOrientation.End, "End path Node");

                startPathNodeDict.Add(startPathPosition, newStartPathNode);
                endPathNodeDict.Add(endPathPosition, newEndPathNode);

                startPathNodeList.Add(newStartPathNode);
                endPathNodeList.Add(newEndPathNode);
            }
        }

        public PathNode GetStartNodeAt(Vector3 position) => startPathNodeDict.GetValueOrDefault(position);
        public PathNode GetEndNodeAt(Vector3 position) => endPathNodeDict.GetValueOrDefault(position);

        public void ConnectPathNodes()
        {
            foreach (PathNode endPathNode in endPathNodeList)
            {
                foreach (PathNode startPathNode in startPathNodeList)
                {
                    endPathNode.AddPathNode(startPathNode);
                }
            }
        }

        public float GetNodeSizeForRoad(RoadObject roadObject) {
            if (!HasIntersection) return 0;

            Dictionary<float, RoadObject> adjacentRoads = GetAdjacentRoadsTo(roadObject);
            float offset;
            float cosAngle;
            int width = roadObject.RoadWidth / 2;
            if (adjacentRoads.Count == 1) {                
                float angle = adjacentRoads.First().Key;
                if (angle > 180) angle = Mathf.Abs(angle - 360);
                angle = Mathf.Clamp(angle, 0, 90);
                angle *= Mathf.Deg2Rad;
                cosAngle = Mathf.Cos(angle - Mathf.PI / 2);
                offset = (1 + Mathf.Cos(angle)) * (width + 0.15f) / cosAngle;
                return offset;
            }

            float leftAngle = adjacentRoads.First().Key;
            float rightAngle = adjacentRoads.Last().Key;
            float smallestAngle;

            if (rightAngle > 180) rightAngle = Mathf.Abs(rightAngle - 360);
            smallestAngle = Mathf.Min(leftAngle, rightAngle);
            smallestAngle = Mathf.Clamp(smallestAngle, 0, 90);
            smallestAngle *= Mathf.Deg2Rad;
            cosAngle = Mathf.Cos(smallestAngle - Mathf.PI / 2);
            offset = (1 + Mathf.Cos(smallestAngle)) * (width + 0.15f) / cosAngle;
          
            return offset;
        }
        public void SetMesh()
        {
            Mesh mesh = NodeMeshBuilder.CreateNodeMesh(this);
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            // Material tiling will depend on the road lengh, so let's have
            // different instances
            meshRenderer.material = new Material(connectedRoadsList[0].GetRoadObjectSO.roadMaterial);

            meshRenderer.material.mainTextureScale = new Vector2(.5f, 1);
            meshRenderer.material.mainTextureOffset = new Vector2(0, 0);
        }

        public bool HasIntersection => connectedRoadsList.Count > 1;

        public void AddRoad(RoadObject roadObject) 
        {
            if (!connectedRoadsList.Contains(roadObject))
            {
                connectedRoadsList.Add(roadObject);
                CreatePathNodeFor(roadObject);
                CreateMeshEdjes(roadObject);
                foreach (RoadObject connectedRoad in GetAdjacentRoadsTo(roadObject).Values)
                {
                    UpdateEdjePositions(connectedRoad);
                }
            }
        }

        private void CreateMeshEdjes(RoadObject roadObject)
        {
            EdjePosition edjePosition;
            bool isStartNode = IsStartNodeOf(roadObject);
            MeshEdje centerEdje = Instantiate(
                meshEdjePrefab,
                transform.position,
                Quaternion.identity,
                transform
                ).GetComponent<MeshEdje>();

            if (isStartNode)
                edjePosition = EdjePosition.StartCenter;
            else
                edjePosition = EdjePosition.EndCenter;
            centerEdje.Init(edjePosition);

            MeshEdje leftEdje = Instantiate(
                meshEdjePrefab,
                transform.position,
                Quaternion.identity,
                transform
                ).GetComponent<MeshEdje>();

            if (isStartNode)
                edjePosition = EdjePosition.StartLeft;
            else
                edjePosition = EdjePosition.EndLeft;

            leftEdje.Init(edjePosition);

            MeshEdje rightEdje = Instantiate(
                meshEdjePrefab,
                transform.position,
                Quaternion.identity,
                transform
                ).GetComponent<MeshEdje>();

            if (isStartNode)
                edjePosition = EdjePosition.StartRight;
            else
                edjePosition = EdjePosition.EndRight;

            rightEdje.Init(edjePosition);

            if (meshEdjesDict.ContainsKey(roadObject))
                meshEdjesDict[roadObject].AddRange(new List<MeshEdje> { centerEdje, leftEdje, rightEdje });
            else
                meshEdjesDict.Add(roadObject, new List<MeshEdje> {centerEdje, leftEdje, rightEdje });
        }

        private bool IsStartNodeOf(RoadObject roadObject)
        {
            return roadObject.StartNode.Equals(this);
        }

        private void CreatePathNodeFor(RoadObject roadObject)
        {
            PathNode newEndPathNode = Instantiate(
                pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PathNode>();

            newEndPathNode.Init(PathNode.PathOrientation.End, "End path Node");
            PathNode newStartPathNode = Instantiate(
                pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PathNode>();

            newStartPathNode.Init(PathNode.PathOrientation.Start, "Start path Node");
            connectedPathNodesDict.Add(roadObject, new List<PathNode> { newStartPathNode, newEndPathNode });
        }

        public void RemoveRoad(RoadObject roadObject, bool keepNodes) {
            if (connectedRoadsList.Contains(roadObject)) {
                connectedRoadsList.Remove(roadObject);
                connectedPathNodesDict.Remove(roadObject);

                if (connectedRoadsList.Count <= 0 && !keepNodes)
                    Destroy(gameObject);
            }
        }

        public List<RoadObject> ConnectedRoads => connectedRoadsList;
        public bool HasConnectedRoads => connectedRoadsList.Count > 0;
        public Vector3 Position => transform.position;
        public Vector3 Direction => Position - connectedRoadsList.First().ControlNodePosition;
        public Dictionary<float, RoadObject> GetAdjacentRoadsTo(RoadObject roadObject) {
            Dictionary<float, RoadObject> connectedRoadsDict = new();

            if (!HasIntersection) return connectedRoadsDict;

            if (HasIntersection && roadObject != null) {
                Vector3 roadObjectDirection = Position - roadObject.ControlNodePosition;

                foreach (RoadObject road in connectedRoadsList) {
                    if (road != roadObject) {
                        Vector3 connectedRoadDirection = Position - road.ControlNodeObject.transform.position;
                        float angle = Vector3.SignedAngle(roadObjectDirection, connectedRoadDirection, transform.up);
                        if (angle < 0) angle += 360;
                        if (!connectedRoadsDict.ContainsKey(angle))
                            connectedRoadsDict.Add(angle, road);
                    }
                }
            }

            connectedRoadsDict = connectedRoadsDict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            Dictionary<float, RoadObject> adjacentRoads = new()
            {
                { connectedRoadsDict.First().Key, connectedRoadsDict.First().Value }
            };

            if (connectedRoadsDict.Count > 1)
                adjacentRoads.Add(connectedRoadsDict.Last().Key, connectedRoadsDict.Last().Value);

            return adjacentRoads;
        }
        public List<Node> GetConnectedNodes()
        {
            List<Node> connectedNodes = new();
            foreach (RoadObject connectedRoad in connectedRoadsList)
            {
                connectedNodes.Add(connectedRoad.OtherNodeTo(this));
            }

            return connectedNodes;
        }

        public bool Equals(Node other)
        {
            return Vector3.SqrMagnitude(Position - other.Position) < 0.0001f;
        }
    }
}
