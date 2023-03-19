using Road.Obj;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Road.NodeObj {
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(SphereCollider))]
    public class Node : MonoBehaviour {
        [SerializeField] private List<RoadObject> connectedRoads = new();

        public float GetNodeSizeForRoad(RoadObject roadObject) {
            if (!HasIntersection()) return 0;

            Dictionary<float, RoadObject> adjacentRoads = GetAdjacentRoadsTo(roadObject);

            float leftAngle = adjacentRoads.First().Key;
            if (leftAngle > 180) leftAngle -= 180;
            float rightAngle = 180;

            if (adjacentRoads.Count > 1) {
                rightAngle = adjacentRoads.Last().Key;
                if (rightAngle > 180) rightAngle -= 180;
            }

            float smallestAngle = Mathf.Min(leftAngle, rightAngle);

            float cosAngle = Mathf.Abs(Mathf.Cos(smallestAngle * Mathf.Deg2Rad));
            Mathf.Clamp01(cosAngle);
            float offset = (1.1f + cosAngle) * roadObject.RoadWidth;
            return offset;
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

        public List<RoadObject> ConnectedRoads() => connectedRoads;

        public Vector3 Position => transform.position;

        public Dictionary<float, RoadObject> GetAdjacentRoadsTo(RoadObject roadObject) {
            Dictionary<float, RoadObject> connectedRoadsDict = new();

            if (HasIntersection() && roadObject != null) {
                Vector3 roadObjectDirection = Position - roadObject.transform.position;

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

            Dictionary<float, RoadObject> adjacentRoads = new();
            adjacentRoads.Add(connectedRoadsDict.First().Key, connectedRoadsDict.First().Value);
            if (connectedRoadsDict.Count > 1)
                adjacentRoads.Add(connectedRoadsDict.Last().Key, connectedRoadsDict.Last().Value);

            return adjacentRoads;
        }
    }
}
