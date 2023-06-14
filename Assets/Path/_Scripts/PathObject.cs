using System;
using UnityEngine;
using System.Collections.Generic;
using Path.Utilities;
using Path.Entities.Pedestrian;
using Path.Entities.SO;
using Paths.MeshHandler;
using Rafael.Utils;

namespace Path.Entities{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public abstract class PathObject : MonoBehaviour, IPath {

        public EventHandler OnPathBuilt;
        public EventHandler OnPathRemoved;
        public EventHandler OnPathUpdated;
        
        [SerializeField] protected PathSO pathSO;
        [SerializeField] private GameObject nodePrefab;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        protected NodeObject startNode;
        protected NodeObject endNode;
        private GameObject controlNodeObject;


        public NodeObject OtherNodeTo(NodeObject thisNode) {
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
            if (PathManager.Instance.updatePaths) {
                // controlNodeObject.transform.position = (startNode.Position + endNode.Position) / 2;
                UpdateMesh();
            }
        }

        public int Width => pathSO.Width;
        public int Resolution => pathSO.resolution;
        public PathSO PathSO => pathSO;
        public Vector3 ControlPosition => controlNodeObject.transform.position;
        public NodeObject StartNode => startNode;
        public NodeObject EndNode => endNode;
        public GameObject NodePrefab => nodePrefab;

        public void SetMesh()
        {
            Mesh pathMesh = pathSO.CreatePathMesh(this);
            Mesh startNodeMesh = pathSO.CreateNodeMesh(startNode);
            Mesh endNodeMesh = pathSO.CreateNodeMesh(endNode);

            SetPathMesh(pathMesh);
            startNode.SetMesh(startNodeMesh);
            endNode.SetMesh(endNodeMesh);
        }

        protected virtual void ConnectPathNodes() {}

        public void SetPathMesh(Mesh mesh) {
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            // Material tiling will depend on the path lengh, so let's have
            // different instances
            meshRenderer.material = new Material(pathSO.material);

            float pathLengh = Bezier.GetLengh(startNode.Position, endNode.Position, ControlPosition);
            int textureRepead = Mathf.RoundToInt(pathSO.textureTiling * pathLengh * .01f);
            meshRenderer.material.mainTextureScale = new Vector2(.5f, textureRepead);
            meshRenderer.material.mainTextureOffset = new Vector2(0, 0);

        }

        public void UpdateMesh()
        {
            SetMesh();
        }

        public List<PathObject> GetAllConnectedPaths() {
            List<PathObject> startNodeConnections = startNode.ConnectedPaths;
            List<PathObject> endNodeConnections = endNode.ConnectedPaths;

            List<PathObject> allConnectedPaths = new();
            allConnectedPaths.AddRange(startNodeConnections);
            allConnectedPaths.AddRange(endNodeConnections);

            allConnectedPaths.Remove(this);

            return allConnectedPaths;
        }

        # region CRUD
        public virtual void PlacePath(NodeObject startNode, NodeObject endNode, Vector3 controlPosition)
        {
            this.startNode = startNode;
            this.endNode = endNode;

            controlNodeObject = RafaelUtils.CreateSphere(controlPosition, "Control Node", transform, 1f);

            this.startNode.AddPath(this);
            this.endNode.AddPath(this);

            ConnectPathNodes();
            SetMesh();
            foreach (PathObject connectedPath in startNode.GetAdjacentPathsTo(this).Values)
            {
                connectedPath.UpdateMesh();
            }
            foreach (PathObject connectedPath in endNode.GetAdjacentPathsTo(this).Values)
            {
                connectedPath.UpdateMesh();
            }
        }

        public void BuildPath()
        {
            throw new NotImplementedException();
        }

        public void UpdatePath()
        {
            throw new NotImplementedException();
        }

        public void RemovePath()
        {
            OnPathRemoved?.Invoke(this, EventArgs.Empty);
            OnPathRemoved = null;
            OnPathBuilt = null;
            OnPathUpdated = null;
            Destroy(gameObject);
        }
        #endregion
    }
}
