using Path.Entities;
using Paths.MeshHandler.Data;
using Paths.MeshHandler;
using UnityEngine;

namespace Path.Entities.SO
{
    public class PathSO : ScriptableObject
    {
        public GameObject pathObjectPrefab;
        public Material material;

        public int width;
        public int resolution;
        public int textureTiling;

        public int minIntersectionAngle;

        public static Mesh CreatePathMesh(PathObject pathObject)
        {
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
