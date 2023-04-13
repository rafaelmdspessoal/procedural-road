using System;
using UnityEngine;
using System.Collections.Generic;
using Nodes;
using Roads.Manager;
using Roads.MeshHandler;
using Rafael.Utils;
using Roads.Utilities;

namespace Roads {
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

        private GameObject startMeshCenterGO;
        private GameObject startMeshLeftGO;
        private GameObject startMeshRightGO;


        private GameObject endMeshCenterGO;
        private GameObject endMeshLeftGO;
        private GameObject endMeshRightGO;
        public Vector3 startMeshCenterPosition { 
            get { return startMeshCenterGO.transform.position - transform.position; } 
        }
        public Vector3 startMeshLeftPosition { 
            get { return startMeshLeftGO.transform.position - transform.position; } 
        }
        public Vector3 startMeshRightPosition { 
            get { return startMeshRightGO.transform.position - transform.position; } 
        }
        public Vector3 endMeshCenterPosition { 
            get { return endMeshCenterGO.transform.position - transform.position; } 
        }
        public Vector3 endMeshLeftPosition { 
            get { return endMeshLeftGO.transform.position - transform.position; } 
        }
        public Vector3 endMeshRightPosition { 
            get { return endMeshRightGO.transform.position - transform.position; } 
        }
        public Vector3 controlPosition { 
            get { return controlNodeObject.transform.position - transform.position; } 
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

        public void PlaceRoad(Node startNode, Node endNode, GameObject controlNodeObject)
        {
            this.startNode = startNode;
            this.endNode = endNode;
            this.controlNodeObject = controlNodeObject;
            this.startNode.AddRoad(this);
            this.endNode.AddRoad(this);

            controlNodeObject.transform.parent = transform;
            Debug.Log("Road Placed!");

            PlaceRoadEdjes();
            UpdateMeshEdjes();
            OnRoadPlaced?.Invoke(this, new OnRoadChangedEventArgs { roadObject = this });
        }

        private void PlaceRoadEdjes()
        {
            startMeshCenterGO = RafaelUtils.CreateSphere(
                Vector3.zero,
                "startCenterMeshPosition",
                this.transform);

            startMeshLeftGO = RafaelUtils.CreateSphere(
                Vector3.zero,
                "startLeftMeshPosition",
                this.transform);
            startMeshRightGO = RafaelUtils.CreateSphere(
                Vector3.zero,
                "startRightMeshPosition",
                this.transform);

            endMeshCenterGO = RafaelUtils.CreateSphere(
                Vector3.zero,
                "endCenterMeshPosition",
                this.transform);

            endMeshLeftGO = RafaelUtils.CreateSphere(
                Vector3.zero,
                "endLeftMeshPosition",
                this.transform);

            endMeshRightGO = RafaelUtils.CreateSphere(
                Vector3.zero,
                "endRightMeshPosition",
                this.transform);
        }

        private void UpdateMeshEdjes()
        {
            Vector3 startCenterMeshPosition = startNode.Position;
            Vector3 endCenterMeshPosition = endNode.Position;

            if (startNode.HasIntersection())
            {
                float roadOffsetDistance = startNode.GetNodeSizeForRoad(this);
                startCenterMeshPosition = Bezier.GetOffsettedPosition(
                    startNode.Position,
                    endNode.Position,
                    controlNodeObject.transform.position,
                    roadOffsetDistance);
            }
            if (endNode.HasIntersection())
            {
                float roadOffsetDistance = endNode.GetNodeSizeForRoad(this);
                endCenterMeshPosition = Bezier.GetOffsettedPosition(
                    endNode.Position,
                    startNode.Position,
                    controlNodeObject.transform.position,
                    roadOffsetDistance);
            }
            Vector3 startLeftMeshPosition = RoadUtilities.GetRoadLeftSideVertice(
                roadObjectSO.roadWidth,
                startCenterMeshPosition,
                controlNodeObject.transform.position);
            Vector3 startRightMeshPosition = RoadUtilities.GetRoadRightSideVertice(
               roadObjectSO.roadWidth,
               startCenterMeshPosition,
               controlNodeObject.transform.position);

            Vector3 endLeftMeshPosition = RoadUtilities.GetRoadLeftSideVertice(
               roadObjectSO.roadWidth,
               endCenterMeshPosition,
               controlNodeObject.transform.position);
            Vector3 endRightMeshPosition = RoadUtilities.GetRoadRightSideVertice(
               roadObjectSO.roadWidth,
               endCenterMeshPosition,
               controlNodeObject.transform.position);

            startMeshCenterGO.transform.position = startCenterMeshPosition;
            startMeshLeftGO.transform.position = startLeftMeshPosition;
            startMeshRightGO.transform.position = startRightMeshPosition;

            endMeshCenterGO.transform.position = endCenterMeshPosition;
            endMeshLeftGO.transform.position = endLeftMeshPosition;
            endMeshRightGO.transform.position = endRightMeshPosition;
        }

        public void SetMesh()
        {
            UpdateMeshEdjes();
            SetRoadMesh();
            startNode.SetMesh();
            endNode.SetMesh();
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
            UpdateMeshEdjes();
            SetRoadMesh();
            startNode.SetMesh();
            endNode.SetMesh();
        }

        public void Remove(bool keepNodes) {
            StartNode.RemoveRoad(this, keepNodes);
            EndNode.RemoveRoad(this, keepNodes);

            OnRoadRemoved?.Invoke(this, new OnRoadChangedEventArgs { roadObject = this });
            OnRoadRemoved = null;

            foreach (RoadObject roadToUpdate in GetAllConnectedRoads())
            {
                roadToUpdate.UpdateMesh();
            }

            Destroy(gameObject);
        }

        public int RoadWidth => roadObjectSO.roadWidth;
        public int RoadResolution => roadObjectSO.roadResolution;
        public RoadObjectSO GetRoadObjectSO => roadObjectSO;
        public Vector3 ControlPosition => controlNodeObject.transform.position;
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
