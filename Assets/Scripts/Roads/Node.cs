using Roads;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Nodes.MeshHandler;

namespace Nodes {
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    public class Node : MonoBehaviour {

        [SerializeField] private List<RoadObject> connectedRoads = new();

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
        }

        public float GetNodeSizeForRoad(RoadObject roadObject) {
            if (!HasIntersection()) return 0;

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
            meshRenderer.material = new Material(connectedRoads[0].GetRoadObjectSO.roadMaterial);

            meshRenderer.material.mainTextureScale = new Vector2(.5f, 1);
            meshRenderer.material.mainTextureOffset = new Vector2(0, 0);
        }

        public bool HasIntersection() => connectedRoads.Count > 1;

        public void AddRoad(RoadObject segment) {
            if (!connectedRoads.Contains(segment)) {
                connectedRoads.Add(segment);
                segment.transform.name = "Road number " + connectedRoads.Count;
            }

        }

        public void RemoveRoad(RoadObject roadObject, bool keepNodes) {
            if (connectedRoads.Contains(roadObject)) {
                connectedRoads.Remove(roadObject);

                if (connectedRoads.Count <= 0 && !keepNodes)
                    Destroy(gameObject);
            }
        }

        public List<RoadObject> ConnectedRoads => connectedRoads;
        public bool HasConnectedRoads => connectedRoads.Count > 0;
        public Vector3 Position => transform.position;
        public Vector3 Direction => Position - connectedRoads.First().ControlNodePosition;
        public Dictionary<float, RoadObject> GetAdjacentRoadsTo(RoadObject roadObject) {
            Dictionary<float, RoadObject> connectedRoadsDict = new();

            if (HasIntersection() && roadObject != null) {
                Vector3 roadObjectDirection = Position - roadObject.ControlNodePosition;

                foreach (RoadObject road in connectedRoads) {
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
    }
}
