using System;
using UnityEngine;
using System.Collections.Generic;
using Path.Utilities;
using Path.Entities.Pedestrian;
using Path.Entities.SO;
using Paths.MeshHandler;

namespace Path.Entities{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class PathObject : MonoBehaviour, IPath {

        public EventHandler OnPathPlaced;
        public EventHandler OnPathBuilt;
        public EventHandler OnPathRemoved;
        public EventHandler OnPathUpdated;
        
        [SerializeField] private PathSO pathSO;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        private NodeObject startNode;
        private NodeObject endNode;
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
                controlNodeObject.transform.position = (startNode.Position + endNode.Position) / 2;
                UpdateMesh();
            }
        }

        public int Width => pathSO.width;
        public int Resolution => pathSO.resolution;
        public PathSO PathSO => pathSO;
        public Vector3 ControlPosition => controlNodeObject.transform.position;
        public NodeObject StartNode => startNode;
        public NodeObject EndNode => endNode;

        public void Init(NodeObject startNode, NodeObject endNode, Vector3 controlPosition)
        {
            this.startNode = startNode;
            this.endNode = endNode;

            controlNodeObject = PathUtilities.CreateControlNode(pathSO, controlPosition);
            controlNodeObject.transform.parent = transform;

            this.startNode.AddPath(this);
            this.endNode.AddPath(this);

            startNode.Init(this);
            endNode.Init(this);

            ConnectPathNodes();

            OnPathPlaced?.Invoke(this, EventArgs.Empty);
        }

        public void SetMesh()
        {
            SetPathMesh();
            startNode.SetMesh();
            endNode.SetMesh();
        }

        private void ConnectPathNodes()
        {
            PedestrianPathNode startNodeStartPath = startNode.GetPathNodeFor(this, PedestrianPathNode.OnPathPosition.StartNodeStartPath);
            PedestrianPathNode startNodeEndPath = startNode.GetPathNodeFor(this, PedestrianPathNode.OnPathPosition.StartNodeEndPath);

            PedestrianPathNode endNodeStartPath = endNode.GetPathNodeFor(this, PedestrianPathNode.OnPathPosition.EndNodeStartPath);
            PedestrianPathNode endNodeEndPath = endNode.GetPathNodeFor(this, PedestrianPathNode.OnPathPosition.EndNodeEndPath);

            startNodeStartPath.AddPathNode(endNodeEndPath);
            endNodeStartPath.AddPathNode(startNodeEndPath);
        }

        public void SetPathMesh() {
            Mesh mesh = PathMeshBuilder.CreatePathMesh(this);
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
            SetPathMesh();
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
        public void PlacePath(NodeObject startNode, NodeObject endNode, Vector3 controlPosition)
        {
            throw new NotImplementedException();
        }

        public void BuildPath()
        {
            throw new NotImplementedException();
        }

        public void UpdatePath()
        {
            throw new NotImplementedException();
        }

        public void RemovePath(bool keepNodes)
        {
            startNode.RemovePath(this, keepNodes);
            endNode.RemovePath(this, keepNodes);

            OnPathRemoved?.Invoke(this, EventArgs.Empty);

            OnPathRemoved = null;
            OnPathPlaced = null;
            OnPathBuilt = null;
            OnPathUpdated = null;

            Destroy(gameObject);
        }
        #endregion
    }
}
