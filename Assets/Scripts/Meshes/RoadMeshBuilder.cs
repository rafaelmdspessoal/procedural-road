using UnityEngine;
using MeshHandler.Road.NodeMeshData;
using MeshHandler.Road.RoadMeshData;
using MeshHandler.Utilities;
using Road.Obj;

namespace MeshHandler.Road.Builder {
    public class RoadMeshBuilder {

        /// <summary>
        /// Creates the Mesh for the given roadObject
        /// </summary>
        /// <param name="roadObject"></param>
        /// <returns></returns>
        public static Mesh CreateRoadMesh(RoadObject roadObject) {
            MeshData meshData = PopulateMeshData(roadObject);
            Mesh mesh = MeshUtilities.LoadMesh(meshData);
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
        private static MeshData PopulateMeshData(RoadObject roadObject) {
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
    }
}
 
