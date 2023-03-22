using System;
using UnityEngine;
using MeshHandler.Road.Builder;
using System.Collections.Generic;
using Road.NodeObj;
using Road.Manager;

namespace Road.Obj {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class RoadObject : MonoBehaviour, IRemoveable {

        public EventHandler<OnRoadChangedEventArgs> OnRoadPlaced;
        public EventHandler<OnRoadChangedEventArgs> OnRoadBuilt;
        public EventHandler<OnRoadChangedEventArgs> OnRoadRemoved;
        public EventHandler<OnRoadChangedEventArgs> OnRoadUpdated;

        public class OnRoadChangedEventArgs : EventArgs { public RoadObject roadObject; }
        
        [SerializeField] private RoadObjectSO roadObjectSO;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private Node startNode;
        private Node endNode;
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
                UpdateRoadMesh();
                //controlNodeObject.transform.position = (startNode.Position + endNode.Position) / 2;
            }
        }

        public void PlaceRoad(Node startNode, Node endNode, GameObject controlNodeObject) {
            this.startNode = startNode;
            this.endNode = endNode;
            this.controlNodeObject = controlNodeObject;
            this.startNode.AddRoad(this);
            this.endNode.AddRoad(this);

            this.startNode.transform.localScale = RoadWidth * Vector3Int.one;
            this.endNode.transform.localScale = RoadWidth * Vector3Int.one;

            controlNodeObject.transform.parent = transform;
            Debug.Log("Road Placed!");
            SetRoadMesh();
            OnRoadPlaced?.Invoke(this, new OnRoadChangedEventArgs { roadObject = this });
            foreach (RoadObject roadObj in GetAllConnectedRoads()) {
                roadObj.UpdateRoadMesh();
            }
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

        public void UpdateRoadMesh() {
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

        public void Remove(bool keepNodes) {
            StartNode.RemoveRoad(this, keepNodes);
            EndNode.RemoveRoad(this, keepNodes);

            OnRoadRemoved?.Invoke(this, new OnRoadChangedEventArgs { roadObject = this });
            OnRoadRemoved = null;

            Destroy(gameObject);
        }

        public void Remove() {
            StartNode.RemoveRoad(this, false);
            EndNode.RemoveRoad(this, false);

            OnRoadRemoved?.Invoke(this, new OnRoadChangedEventArgs { roadObject = this });
            OnRoadRemoved = null;

            Destroy(gameObject);
        }

        public int RoadWidth => roadObjectSO.roadWidth;

        public int RoadResolution => roadObjectSO.roadResolution;

        public RoadObjectSO GetRoadObjectSO => roadObjectSO;

        public List<RoadObject> GetAllConnectedRoads() {
            List<RoadObject> startNodeConnections = startNode.ConnectedRoads();
            List<RoadObject> endNodeConnections = endNode.ConnectedRoads();

            List<RoadObject> allConnectedRoads = new();
            allConnectedRoads.AddRange(startNodeConnections);
            allConnectedRoads.AddRange(endNodeConnections);

            allConnectedRoads.Remove(this);

            return allConnectedRoads;
        }
    }
}
