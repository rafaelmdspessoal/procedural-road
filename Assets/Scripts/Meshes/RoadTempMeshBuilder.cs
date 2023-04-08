using UnityEngine;
using Roads.MeshHandler;

namespace Roads.Preview.MeshHandler 
{
    public class RoadTempMeshBuilder {

        public static Mesh CreateTempRoadMesh(
            Vector3 startPosition,
            Vector3 endPosition,
            Vector3 controlPosition,
            int roadWidth,
            int resolution) 
        {
            MeshData meshData = new();
            PreviewRoadMeshData displayRoadMeshData = new(startPosition, endPosition, controlPosition, resolution, roadWidth);
            meshData = displayRoadMeshData.PopulateTempRoadMeshVertices(meshData);
            Mesh mesh = MeshUtilities.LoadMesh(meshData);
            return mesh;
        }
    }
}
