using UnityEngine;
using Roads.MeshHandler.Data;

namespace Roads.MeshHandler {
    public class RoadMeshBuilder {

        /// <summary>
        /// Creates the Mesh for the given roadObject
        /// </summary>
        /// <param name="roadObject"></param>
        /// <returns></returns>
        public static Mesh CreateRoadMesh(RoadObject roadObject) {
            MeshData meshData = new();
            RoadMeshData roadMeshData = new(roadObject);
            roadMeshData.PopulateRoadMeshVertices(meshData);

            MeshUtilities.PopulateMeshTriangles(meshData);
            MeshUtilities.PopulateMeshUvs(meshData);

            Mesh mesh = MeshUtilities.LoadMesh(meshData);
            return mesh;
        }
    }
}
 
