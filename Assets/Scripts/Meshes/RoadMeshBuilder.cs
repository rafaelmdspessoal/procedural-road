using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadMeshBuilder : MonoBehaviour {

    public static RoadMeshBuilder Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public Mesh CreateRoadMesh(float roadWidth, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition) {
        MeshData meshData = PopulateMeshData(roadWidth, startPosition, endPosition, controlPosition);
        Mesh mesh = LoadMesh(meshData);
        return mesh;
    }

    public Mesh CreateRoadMesh(RoadObject roadObject) {
        MeshData meshData = PopulateMeshData(roadObject);
        Mesh mesh = LoadMesh(meshData);
        return mesh;
    }

    private MeshData PopulateMeshData(RoadObject roadObject) {
        Node startNode = roadObject.StartNode;
        Node endNode = roadObject.EndNode;

        MeshData meshData = new MeshData();

        MeshUtilities.PopulateNodeMeshVertices(meshData, roadObject, startNode, endNode);
        MeshUtilities.PopulateRoadMeshVertices(meshData, roadObject);
        MeshUtilities.PopulateNodeMeshVertices(meshData, roadObject, endNode, startNode);

        MeshUtilities.PopulateMeshTriangles(meshData);
        MeshUtilities.PopulateMeshUvs(meshData);

        return meshData;
    }

    private MeshData PopulateMeshData(float roadWidth, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition) {

        MeshData meshData = new MeshData();
        MeshUtilities.PopulateNodeMeshVertices(meshData, roadWidth, startPosition, endPosition, controlPosition);
        MeshUtilities.PopulateRoadMeshVertices(meshData, roadWidth, startPosition, endPosition, controlPosition);
        MeshUtilities.PopulateNodeMeshVertices(meshData, roadWidth, endPosition, startPosition, controlPosition);

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
 
