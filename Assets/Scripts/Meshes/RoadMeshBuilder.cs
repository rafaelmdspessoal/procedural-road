using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Road.Mesh.NodeVertices;

public class RoadMeshBuilder : MonoBehaviour {

    public static RoadMeshBuilder Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public Mesh CreateRoadMesh(int roadWidth, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int resolution = 20) {
        MeshData meshData = PopulateMeshData(roadWidth, startPosition, endPosition, controlPosition, resolution);
        Mesh mesh = LoadMesh(meshData);
        return mesh;
    }

    public Mesh CreateRoadMesh(RoadObject roadObject) {
        MeshData meshData = PopulateMeshData(roadObject);
        Mesh mesh = LoadMesh(meshData);
        return mesh;
    }

    private MeshData PopulateMeshData(RoadObject roadObject, int resolution = 10) {
        Node startNode = roadObject.StartNode;
        Node endNode = roadObject.EndNode;

        MeshData meshData = new MeshData();

        new CalculateNodeWIMeshData(roadObject, startNode, endNode, resolution).PopulateStartNode(meshData);
        CalculateRoadMeshData.PopulateRoadMeshVertices(meshData, roadObject);
        new CalculateNodeWIMeshData(roadObject, startNode, endNode, resolution).PopulateEndNode(meshData);

        MeshUtilities.PopulateMeshTriangles(meshData);
        MeshUtilities.PopulateMeshUvs(meshData);

        return meshData;
    }

    // NOTE: This is only used to populate temporary road for display when building
    // Consider having a module just for this.
    private MeshData PopulateMeshData(int roadWidth, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int resolution = 20) {

        MeshData meshData = new MeshData();
        CalculateNodeWOIMeshData.PopulateStartNode(meshData, roadWidth, startPosition, controlPosition, resolution);
        CalculateRoadMeshData.PopulateRoadMeshVertices(meshData, roadWidth, startPosition, endPosition, controlPosition, resolution);
        CalculateNodeWOIMeshData.PopulateEndNode(meshData, roadWidth, endPosition, controlPosition, resolution);

        MeshUtilities.PopulateMeshTriangles(meshData);
        MeshUtilities.PopulateMeshUvs(meshData);

        return meshData;
    }
    

    private Mesh LoadMesh(MeshData meshData)
    {
        Mesh mesh = LoadMeshData(meshData);
        return mesh;
    }

    private Mesh LoadMeshData(MeshData meshData) {
        /*
        Debug.Log("Vets: " + meshData.vertices.Count);
        Debug.Log("Uvs: " + meshData.uvs.Count);
        Debug.Log("Tris: " + meshData.triangles.Count);
        */
        Mesh mesh = new Mesh() {
            vertices = meshData.vertices.ToArray(),
            uv = meshData.uvs.ToArray(),
            triangles = meshData.triangles.ToArray(),
        };
        return mesh;
    }

}
 
