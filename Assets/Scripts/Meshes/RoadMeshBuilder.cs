using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Road.Mesh.RoadData.Visual;
using Road.Mesh.RoadData;
using Road.Mesh.NodeData;


public class RoadMeshBuilder : MonoBehaviour {

    public static RoadMeshBuilder Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public Mesh CreateRoadMesh(int roadWidth, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int resolution = 10) {
        MeshData meshData = PopulateDisplayRoadMeshData(startPosition, endPosition, controlPosition, resolution, roadWidth);
        Mesh mesh = LoadMesh(meshData);
        return mesh;
    }

    public Mesh CreateRoadMesh(RoadObject roadObject) {
        MeshData meshData = PopulateMeshData(roadObject);
        Mesh mesh = LoadMesh(meshData);
        return mesh;
    }

    private MeshData PopulateMeshData(RoadObject roadObject) {        
        MeshData meshData = new();
        PopulateRoadMeshData roadMeshData = new(roadObject);
        PopulateNodeMeshData nodeMeshData = new(roadObject);

        nodeMeshData.PopulateStartNodeMesh(meshData);
        roadMeshData.PopulateRoadMeshVertices(meshData);
        nodeMeshData.PopulateEndNodeMesh(meshData);

        MeshUtilities.PopulateMeshTriangles(meshData);
        MeshUtilities.PopulateMeshUvs(meshData);

        return meshData;
    }

    // NOTE: This is only used to populate temporary road for display when building
    // Consider having a module just for this.
    private MeshData PopulateDisplayRoadMeshData(
        Vector3 startPosition,
        Vector3 endPosition,
        Vector3 controlPosition,
        int roadWidth,
        int resolution) {

        MeshData meshData = new();
        CalculateRoadTemporaryMesh displayRoadMeshData = new(startPosition, endPosition, controlPosition, resolution, roadWidth);

        meshData = displayRoadMeshData.PopulateRoadMeshVertices(meshData);

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
        Mesh mesh = new() {
            vertices = meshData.vertices.ToArray(),
            uv = meshData.uvs.ToArray(),
            triangles = meshData.triangles.ToArray(),
        };
        return mesh;
    }

}
 
