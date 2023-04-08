using Roads.Placement;
using Roads.Preview.MeshHandler;
using System.Collections;
using System.Collections.Generic;
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
            Vector3 endPosition,
            Vector3 controlPosition,
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

    }
}