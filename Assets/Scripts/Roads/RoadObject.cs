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
        private Node startNode;
        private Node endNode;
        private GameObject controlNodeObject;

        private GameObject startMeshCenterGO;

        private GameObject endMeshCenterGO;

        public Vector3 StartMeshCenterPostion => startMeshCenterGO.transform.position;
        public Vector3 EndMeshCenterPostion => endMeshCenterGO.transform.position;

        public Vector3 ControlPosition() { 
            return controlNodeObject.transform.position - transform.position; 
        }

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
                //controlNodeObject.transform.position = (startNode.Position + endNode.Position) / 2;
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

            // PlaceRoadEdjes();
            // UpdateMeshEdjes();
            OnRoadPlaced?.Invoke(this, EventArgs.Empty);
        }

        public void SetMesh()
        {
            // UpdateMeshEdjes();
            SetRoadMesh();
            startNode.SetMesh();
            //startNode.SetPathPostions();
            //startNode.ConnectPathNodes();

            endNode.SetMesh();
            // endNode.SetPathPostions();
            //endNode.ConnectPathNodes();

            // ConnectRoadPathNodes();
        }

        private void ConnectRoadPathNodes()
        {
            PathNode startNodeStartPath;
            PathNode startNodeEndPath;

            PathNode endNodeStartPath;
            PathNode endNodeEndPath;

            Vector3 dir = StartMeshCenterPostion - startNode.Position;
            Vector3 left = new Vector3(-dir.z, dir.y, dir.x);
            Vector3 startPathPosition = StartMeshCenterPostion - left.normalized;
            Vector3 endPathPosition = StartMeshCenterPostion + left.normalized;

            startNodeStartPath = startNode.GetStartNodeAt(startPathPosition);
            startNodeEndPath = startNode.GetEndNodeAt(endPathPosition);

            dir = EndMeshCenterPostion - endNode.Position;
            left = new Vector3(-dir.z, dir.y, dir.x);
            startPathPosition = EndMeshCenterPostion - left.normalized;
            endPathPosition = EndMeshCenterPostion + left.normalized;

            endNodeStartPath = endNode.GetStartNodeAt(startPathPosition);
            endNodeEndPath = endNode.GetEndNodeAt(endPathPosition);

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
            // UpdateMeshEdjes();
            SetRoadMesh();
            startNode.SetMesh();
            // startNode.SetPathPostions();
            // startNode.ConnectPathNodes();

            endNode.SetMesh();
            // endNode.SetPathPostions();
            // endNode.ConnectPathNodes();
        }

        public void Remove(bool keepNodes) {
            StartNode.RemoveRoad(this, keepNodes);
            EndNode.RemoveRoad(this, keepNodes);

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
