using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(SphereCollider))]
public class Node : MonoBehaviour
{
    [SerializeField] private List<RoadObject> connectedRoads = new();

    // FOR TESTING ONLY
    private RoadObject roadObjectForTesting;


    public float GetNodeSizeForRoad(RoadObject roadObject) {
        if (!HasIntersection()) return 0;

        
        Dictionary<float, RoadObject> adjacentRoads = GetAdjacentRoadsTo(roadObject);

        float smallestAngle = 180f;
        foreach (float roadAngle in adjacentRoads.Keys) {
            RoadObject road = adjacentRoads.GetValueOrDefault(roadAngle);
            if (road != roadObject) {
                if (Mathf.Abs(roadAngle) < smallestAngle) {
                    smallestAngle = Mathf.Abs(roadAngle);
                }
            }
        }
        float cosAngle = Mathf.Cos(smallestAngle*Mathf.Deg2Rad);
        Mathf.Clamp01(cosAngle);
        float offset = (1.1f + cosAngle) * roadObject.RoadWidth;
        return offset;
    }

    public bool HasIntersection()
    {
        return connectedRoads.Count > 1;
    }

    public void AddRoad(RoadObject segment)
    {
        if (!connectedRoads.Contains(segment)) {
            connectedRoads.Add(segment);
            segment.transform.name = "Road number " + connectedRoads.Count;
        }

    }

    public void RemoveRoad(RoadObject roadObject)
    {
        if (connectedRoads.Contains(roadObject)) {
            connectedRoads.Remove(roadObject);

            //if (connectedRoads.Count <= 0)
            //    Destroy(gameObject);
        }
    }

    public List<RoadObject> GetConnectedRoads()
    {
        return connectedRoads;
    }

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }


    public Dictionary<float, RoadObject> GetAdjacentRoadsTo(RoadObject roadObject) {
        Dictionary<float, RoadObject> adjacentRoads = new Dictionary<float, RoadObject>();

        if (HasIntersection() && roadObject != null) {
            Vector3 roadObjectDirection = this.Position - roadObject.transform.position;
            Vector3 connectedRoadDirection;

            foreach (RoadObject road in connectedRoads) {
                if (road != roadObject) {
                    connectedRoadDirection = this.Position - road.transform.position;
                    float angle = Vector3.SignedAngle(roadObjectDirection, connectedRoadDirection, transform.up);
                    adjacentRoads.Add(angle, road);
                }
            }
        }

        var ordered = adjacentRoads.OrderBy(x => Mathf.Abs(x.Key)).Take(2).ToDictionary(x => x.Key, x => x.Value);

        return ordered;
    }

}
