using UnityEngine;
using Paths;
using Nodes;
using Path.Entities;

namespace Paths.MeshHandler {
    public class MeshUtilities {

        public static MeshData PopulateStartNodeVerticesWOIntersection(
            MeshData meshData,
            int resolution,
            Vector3 startPosition,
            Vector3 endLeft,
            Vector3 controlLeft,
            Vector3 startCenter,
            Vector3 endRight,
            Vector3 controlRight)
        {
            float t;

            for (int i = 0; i < resolution; i++)
            {
                t = i / (float)(resolution - 1);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startPosition, endLeft, controlLeft, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startPosition, endRight, controlRight, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(startCenter);
                meshData.AddVertice(rightPathVertice);
            }
            return meshData;
        }

        /// <summary>
        /// Populates mesh for a starting nodePrefab where the left side of a path meets the right
        /// side of the other
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public static MeshData PopulateNodeMeshVerticesWSIntersection(
            MeshData meshData,
            int resolution,
            Vector3 startLeft,
            Vector3 endLeft,
            Vector3 controlLeft,
            Vector3 startCenter,
            Vector3 endCenter,
            Vector3 controlCenter,
            Vector3 startRight,
            Vector3 endRight,
            Vector3 controlRight) {
            float t;

            for (int i = resolution / 2 - 1; i < resolution - 1; i++) {
                t = i / (float)(resolution - 2);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
                Vector3 centerPathVertice = Bezier.QuadraticCurve(startCenter, endCenter, controlCenter, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(centerPathVertice);
                meshData.AddVertice(rightPathVertice);
            }
            return meshData;
        }

        /// <summary>
        /// Calculate the vertices for starting Node
        /// when the intersection has three or more 
        /// paths.
        /// It will start calculating half way into 
        /// the nodePrefab and end at the begginig of the
        /// Path mesh
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public static MeshData PopulateNodeMeshVerticesWDIntersection(
            MeshData meshData,
            int resolution,
            Vector3 startLeft,
            Vector3 endLeft,
            Vector3 controlLeft,
            Vector3 startCenter,
            Vector3 endCenter,
            Vector3 startRight,
            Vector3 endRight,
            Vector3 controlRight) {
            float t;
            for (int i = resolution / 2 - 1; i < resolution - 1; i++) {
                t = i / (float)(resolution - 2);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startLeft, endLeft, controlLeft, t);
                Vector3 centerPathVertice = Bezier.LinearCurve(startCenter, endCenter, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startRight, endRight, controlRight, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(centerPathVertice);
                meshData.AddVertice(rightPathVertice);
            }
            return meshData;
        }

        public static MeshData PopulatePathMeshVertices(
            MeshData meshData,
            int resolution,
            Vector3 startLeft,
            Vector3 endLeft,
            Vector3 controlLeft,
            Vector3 startCenter,
            Vector3 endCenter,
            Vector3 controlCenter,
            Vector3 startRight,
            Vector3 endRight,
            Vector3 controlRight) {
            float t;

            for (int i = 0; i < resolution; i++) {
                t = i / (float)(resolution - 1);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startLeft, endRight, controlLeft, t);
                Vector3 centerPathVertice = Bezier.QuadraticCurve(startCenter, endCenter, controlCenter, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startRight, endLeft, controlRight, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(centerPathVertice);
                meshData.AddVertice(rightPathVertice);
            }
            return meshData;
        }

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
