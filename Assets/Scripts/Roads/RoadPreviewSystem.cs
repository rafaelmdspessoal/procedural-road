using Roads.Preview.MeshHandler;
using System;
using UnityEngine;


namespace Roads.Preview
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RoadPreviewSystem : MonoBehaviour
    {
        public static RoadPreviewSystem Instance { get; private set; }

        [SerializeField] private Material temporaryRoadMaterial;
        [SerializeField] private Material cantBuildRoadMaterial;

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
            int roadWidth,
            int roadResolution,
            bool canBuildRoad)
        {
            Mesh mesh = RoadTempMeshBuilder.CreateTempRoadMesh(
                startPosition,
                endPosition,
                controlPosition,
                roadResolution,
                roadWidth);

            if (canBuildRoad == false)
                meshRenderer.sharedMaterial = cantBuildRoadMaterial;
            else
                meshRenderer.sharedMaterial = temporaryRoadMaterial;

            meshFilter.mesh = mesh;
        }

        public void StopPreview()
        {
            meshFilter.mesh = null;
        }
    }
}