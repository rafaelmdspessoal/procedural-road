using UnityEngine;

namespace Paths.MeshHandler {
    public class MeshUtilities {

        /// <summary>
        /// Calculate the mesh's UVs for given vertices
        /// </summary>
        /// <param name="meshData"></param>
        internal static void PopulateMeshUvs(MeshData meshData) {
            Vector2[] uvs = new Vector2[3];
            int numUvs = meshData.vertices.Count / 3;
            for (int i = 0; i < numUvs; i++) {
                float completionPercent = i / (float)(numUvs - 1);

                uvs[0] = new Vector2(0, completionPercent);
                uvs[1] = new Vector2(1, completionPercent);
                uvs[2] = new Vector2(0, completionPercent);

                meshData.AddUvs(uvs);
            }
        }

        /// <summary>
        /// Calculate the mesh triangles for given vertices
        /// </summary>
        /// <param name="meshData"></param>
        internal static void PopulateMeshTriangles(MeshData meshData) {
            int[] triangles = new int[12];
            int vertIndex = 0;
            int numTriangles = meshData.vertices.Count / 3;
            for (int i = 0; i < numTriangles - 1; i++) {
                triangles[0] = vertIndex + 0;
                triangles[1] = vertIndex + 1;
                triangles[2] = vertIndex + 3;

                triangles[3] = vertIndex + 1;
                triangles[4] = vertIndex + 4;
                triangles[5] = vertIndex + 3;

                triangles[6] = vertIndex + 1;
                triangles[7] = vertIndex + 2;
                triangles[8] = vertIndex + 4;

                triangles[9] = vertIndex + 2;
                triangles[10] = vertIndex + 5;
                triangles[11] = vertIndex + 4;

                vertIndex += 3;
                meshData.AddTriangles(triangles);
            }
        }

        /// <summary>
        /// Loads the mesh data into a actual Mesh
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public static Mesh LoadMesh(MeshData meshData) {
            Mesh mesh = LoadMeshData(meshData);
            return mesh;
        }

        /// <summary>
        /// Create Mesh properties from tiven mesh data
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public static Mesh LoadMeshData(MeshData meshData) {
            Mesh mesh = new() {
                vertices = meshData.vertices.ToArray(),
                uv = meshData.uvs.ToArray(),
                triangles = meshData.triangles.ToArray(),
            };
            return mesh;
        }
    }
}
