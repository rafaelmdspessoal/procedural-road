using UnityEngine;
using Paths.MeshHandler.Data;
using Path.Entities;

namespace Paths.MeshHandler {
    public class PathMeshBuilder {

        /// <summary>
        /// Creates the Mesh for the given pathObject
        /// </summary>
        /// <param name="pathObject"></param>
        /// <returns></returns>
        public static Mesh CreatePathMesh(PathObject pathObject) {
            MeshData meshData = new();
            PathMeshData pathMeshData = new(pathObject);
            pathMeshData.PopulatePathMeshVertices(meshData);

            MeshUtilities.PopulateMeshTriangles(meshData);
            MeshUtilities.PopulateMeshUvs(meshData);

            Mesh mesh = MeshUtilities.LoadMesh(meshData);
            return mesh;
        }
    }
}
 
