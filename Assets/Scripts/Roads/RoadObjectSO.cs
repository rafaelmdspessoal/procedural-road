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
}
