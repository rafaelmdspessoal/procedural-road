using UnityEngine;
using Road.Mesh.RoadData.Visual;
using MeshHandler.Road.NodeMeshData;
using MeshHandler.Road.RoadMeshData;
using MeshHandler.Utilities;

namespace MeshHandler.Road {
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

        /// <summary>
        /// Creates the Mesh for the given roadObject
        /// </summary>
        /// <param name="roadObject"></param>
        /// <returns></returns>
        public Mesh CreateRoadMesh(RoadObject roadObject) {
            MeshData meshData = PopulateMeshData(roadObject);
            Mesh mesh = LoadMesh(meshData);
            return mesh;
        }

        /// <summary>
        /// Populates the meshData of the given roadObject using
        /// our logic
        /// </summary>
        /// <param name="roadObject">
        /// The Road that will receive this mesh
        /// </param>
        /// <returns></returns>
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

        /// <summary>
        /// Loads the mesh data into a actual Mesh
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        private Mesh LoadMesh(MeshData meshData) {
            Mesh mesh = LoadMeshData(meshData);
            return mesh;
        }

        /// <summary>
        /// Create Mesh properties from tiven mesh data
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        private Mesh LoadMeshData(MeshData meshData) {
            Mesh mesh = new() {
                vertices = meshData.vertices.ToArray(),
                uv = meshData.uvs.ToArray(),
                triangles = meshData.triangles.ToArray(),
            };
            return mesh;
        }
    }
}
 
