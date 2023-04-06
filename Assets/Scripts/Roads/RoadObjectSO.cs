using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoadObjectSO : ScriptableObject
{
    public GameObject roadObjectPrefab;
    public Material roadMaterial;

    public int roadWidth;
    public int roadResolution;
    public int roadTextureTiling;

    public int minIntersectionAngle;

    public float GetMaxNodeSize() {
        float angle = minIntersectionAngle * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angle - Mathf.PI / 2);
        float nodeSize = (1 + Mathf.Cos(angle)) * (roadWidth / 2 + 0.15f) / cosAngle;
        return nodeSize;
    }
}
