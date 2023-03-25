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
            float rightAngle = adjacentRoads.Last().Key;
            float smallestAngle;

            if (leftAngle > 180) smallestAngle = rightAngle;
            else if (rightAngle < 180) smallestAngle = leftAngle;
            else {
                rightAngle = 360 - rightAngle;
                smallestAngle = Mathf.Min(leftAngle, rightAngle);
            }
            smallestAngle = Mathf.Clamp(smallestAngle, 0f, 90);
            float cosAngle = Mathf.Abs(Mathf.Cos(smallestAngle * Mathf.Deg2Rad));
            float offset = (0.55f + cosAngle) * 4f;

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
        public Vector3 Direction => Position - connectedRoads.First().ControlPosition;
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
