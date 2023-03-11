using UnityEngine;
using MeshHandler.Road.Temp.Visual;
using MeshHandler.Utilities;

namespace MeshHandler.Road.Temp.Builder {

    public class RoadTempMeshBuilder {
        private readonly PopulateTempRoadMeshData displayRoadMeshData;

        public RoadTempMeshBuilder(Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int roadWidth, int resolution) {
            displayRoadMeshData = new(startPosition, endPosition, controlPosition, resolution, roadWidth);
        }

        public Mesh CreateTempRoadMesh() {
            MeshData meshData = PopulateDisplayRoadMeshData();
            Mesh mesh = MeshUtilities.LoadMesh(meshData);
            return mesh;
        }

        private MeshData PopulateDisplayRoadMeshData() {
            MeshData meshData = new();
            meshData = displayRoadMeshData.PopulateTempRoadMeshVertices(meshData);
            return meshData;
        }
    }
}
