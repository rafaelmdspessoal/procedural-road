using System;
using UnityEngine;
using System.Collections.Generic;
using Nodes;
using Roads.Manager;
using Roads.MeshHandler;
using Rafael.Utils;
using Roads.Utilities;
using UnityEngine.UIElements;

namespace Roads {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class RoadObject : MonoBehaviour, IRemoveable {

        public EventHandler OnRoadPlaced;
        public EventHandler OnRoadBuilt;
        public EventHandler OnRoadRemoved;
        public EventHandler OnRoadUpdated;
        
        [SerializeField] private RoadObjectSO roadObjectSO;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        [SerializeField] private Node startNode;
        [SerializeField] private Node endNode;
        private GameObject controlNodeObject;

        public Node StartNode { get { return startNode; } }
        public Node EndNode { get { return endNode; } }
        public GameObject ControlNodeObject { get { return controlNodeObject; } }

        public Node OtherNodeTo(Node thisNode) {
            if (thisNode == startNode)
                return endNode;
            return startNode;
        }

        private void Awake() {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
        }

        private void Update() {
            if (RoadManager.Instance.updateRoads) {
                controlNodeObject.transform.position = (startNode.Position + endNode.Position) / 2;
                UpdateMesh();
            }
        }

        public void Init(Node startNode, Node endNode, GameObject controlNodeObject)
        {
            this.startNode = startNode;
            this.endNode = endNode;
            this.controlNodeObject = controlNodeObject;
            this.startNode.AddRoad(this);
            this.endNode.AddRoad(this);

            controlNodeObject.transform.parent = transform;

            startNode.Init(this);
            endNode.Init(this);

            ConnectRoadPathNodes();

            OnRoadPlaced?.Invoke(this, EventArgs.Empty);
        }

        public void SetMesh()
        {
            SetRoadMesh();
            startNode.SetMesh();
            endNode.SetMesh();
        }

        private void ConnectRoadPathNodes()
        {
            PathNode startNodeStartPath = startNode.GetPathNodeFor(this, PathNode.PathPosition.StartNodeStartPath);
            PathNode startNodeEndPath = startNode.GetPathNodeFor(this, PathNode.PathPosition.StartNodeEndPath);

            PathNode endNodeStartPath = endNode.GetPathNodeFor(this, PathNode.PathPosition.EndNodeStartPath);
            PathNode endNodeEndPath = endNode.GetPathNodeFor(this, PathNode.PathPosition.EndNodeEndPath);

            startNodeStartPath.AddPathNode(endNodeEndPath);
            endNodeStartPath.AddPathNode(startNodeEndPath);
        }

        public void SetRoadMesh() {
            Mesh mesh = RoadMeshBuilder.CreateRoadMesh(this);
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            // Material tiling will depend on the road lengh, so let's have
            // different instances
            meshRenderer.material = new Material(roadObjectSO.roadMaterial);

            float roadLengh = Bezier.GetLengh(startNode.transform.position, endNode.transform.position);
            int textureRepead = Mathf.RoundToInt(roadObjectSO.roadTextureTiling * roadLengh * .05f);
            meshRenderer.material.mainTextureScale = new Vector2(.5f, textureRepead);
            meshRenderer.material.mainTextureOffset = new Vector2(0, 0);

        }

        public void UpdateMesh()
        {
            SetRoadMesh();
            startNode.UpdatePathPostions(this);
            endNode.UpdatePathPostions(this);
            startNode.UpdateEdjePositions(this);
            endNode.UpdateEdjePositions(this);

            startNode.SetMesh();
            endNode.SetMesh();
        }

        public void Remove(bool keepNodes) {
            startNode.RemoveRoad(this, keepNodes);
            endNode.RemoveRoad(this, keepNodes);

            OnRoadRemoved?.Invoke(this, EventArgs.Empty);

            OnRoadRemoved = null;
            OnRoadPlaced = null;
            OnRoadBuilt = null;
            OnRoadUpdated = null;

            foreach (RoadObject roadToUpdate in GetAllConnectedRoads())
            {
                roadToUpdate.UpdateMesh();
            }

            Destroy(gameObject);
        }

        public int RoadWidth => roadObjectSO.roadWidth;
        public int RoadResolution => roadObjectSO.roadResolution;
        public RoadObjectSO GetRoadObjectSO => roadObjectSO;
        public Vector3 ControlNodePosition => controlNodeObject.transform.position;
        public List<RoadObject> GetAllConnectedRoads() {
            List<RoadObject> startNodeConnections = startNode.ConnectedRoads;
            List<RoadObject> endNodeConnections = endNode.ConnectedRoads;

            List<RoadObject> allConnectedRoads = new();
            allConnectedRoads.AddRange(startNodeConnections);
            allConnectedRoads.AddRange(endNodeConnections);

            allConnectedRoads.Remove(this);

            return allConnectedRoads;
        }
    }
}
