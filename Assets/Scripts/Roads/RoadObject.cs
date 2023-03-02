using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class RoadObject : MonoBehaviour
{
    [SerializeField] private RoadObjectSO roadObjectSO;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Node startNode;
    private Node endNode;
    private GameObject controlNodeObject;

    public Node StartNode { get { return startNode; }}
    public Node EndNode { get { return endNode; } }
    public GameObject ControlNodeObject { get { return controlNodeObject; } }

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void Init(Node startNode, Node endNode, GameObject controlNodeObject) {
        this.startNode = startNode;
        this.endNode = endNode;
        this.controlNodeObject = controlNodeObject;
        controlNodeObject.transform.parent = this.transform;

        this.startNode.AddRoadSegment(this);
        this.endNode.AddRoadSegment(this);
    }

    public void SetRoadMesh(Mesh mesh) {

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

    public float GetRoadWidth() {
        return roadObjectSO.roadWidth;
    }
}
