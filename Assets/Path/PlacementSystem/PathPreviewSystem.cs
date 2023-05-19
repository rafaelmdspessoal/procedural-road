using Paths.MeshHandler;
using Paths.Preview.MeshHandler;
using System;
using UnityEngine;


namespace Path.Preview
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PathPreviewSystem : MonoBehaviour
    {
        public static PathPreviewSystem Instance { get; private set; }

        [SerializeField] private Material temporaryPathMaterial;
        [SerializeField] private Material cantBuildPathMaterial;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private void Awake()
        {
            Instance = this;
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void DisplayTemporaryMesh(
            Vector3 startPosition,
            Vector3 controlPosition,
            Vector3 endPosition,
            int pathWidth,
            int pathResolution,
            bool canBuildPath)
        {
            Mesh mesh = CreateTempPathMesh(
                startPosition,
                endPosition,
                controlPosition,
                pathResolution,
                pathWidth);

            if (canBuildPath == false)
                meshRenderer.sharedMaterial = cantBuildPathMaterial;
            else
                meshRenderer.sharedMaterial = temporaryPathMaterial;

            meshFilter.mesh = mesh;
        }

        public Mesh CreateTempPathMesh(
            Vector3 startPosition,
            Vector3 endPosition,
            Vector3 controlPosition,
            int pathWidth,
            int resolution)
        {
            MeshData meshData = new();
            PreviewPathMeshData displayPathMeshData = new(startPosition, endPosition, controlPosition, resolution, pathWidth);
            meshData = displayPathMeshData.PopulateTempPathMeshVertices(meshData);
            Mesh mesh = MeshUtilities.LoadMesh(meshData);
            return mesh;
        }

        public void StopPreview()
        {
            meshFilter.mesh = null;
        }
    }
}