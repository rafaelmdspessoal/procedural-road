using UnityEngine;
using Paths.MeshHandler;

namespace Paths.Preview.MeshHandler 
{
    public class PathTempMeshBuilder {

        public static Mesh CreateTempPathMesh(
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
    }
}
