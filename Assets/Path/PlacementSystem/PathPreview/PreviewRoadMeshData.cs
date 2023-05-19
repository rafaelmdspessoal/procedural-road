using UnityEngine;
using Paths.MeshHandler;
using Path.Utilities;

namespace Paths.Preview.MeshHandler
{

    public class PreviewPathMeshData {

        private Vector3 startNodePosition;
        private Vector3 endNodePosition;
        private Vector3 controlPosition;

        private readonly int resolution;
        private readonly int pathWidth;

        public PreviewPathMeshData(Vector3 startNodePosition, Vector3 endNodePosition, Vector3 controlPosition, int pathWidth, int resolution) {
            this.startNodePosition = startNodePosition;
            this.endNodePosition = endNodePosition;
            this.controlPosition = controlPosition;
            this.pathWidth = pathWidth;
            this.resolution = resolution;
        }

        public MeshData PopulateTempPathMeshVertices(MeshData meshData) {

            Vector3 startPosition = startNodePosition + (startNodePosition - controlPosition).normalized * pathWidth / 2;
            Vector3 endPosition = endNodePosition + (endNodePosition - controlPosition).normalized * pathWidth / 2;

            Vector3 startLeft = GetPathLeftSideVertice(pathWidth, startNodePosition, controlPosition);
            Vector3 endLeft = GetPathLeftSideVertice(pathWidth, endNodePosition, controlPosition);

            Vector3 startRight = GetPathRightSideVertice(pathWidth, startNodePosition, controlPosition);
            Vector3 endRight = GetPathRightSideVertice(pathWidth, endNodePosition, controlPosition);

            Vector3 controlLeft;
            Vector3 controlRight;

            Vector3 n0 = (startLeft - startNodePosition).normalized;
            Vector3 n1 = (endRight - endNodePosition).normalized;

            if (Vector3.Angle(n0, n1) != 0) {
                // Path is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlLeft = controlPosition + ((n0 + n1) * pathWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
                controlRight = controlPosition - ((n0 + n1) * pathWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
            } else {
                // Path is traight, so calculations are easier
                controlLeft = controlPosition + n0 * pathWidth / 2;
                controlRight = controlPosition - n1 * pathWidth / 2;
            }

            Vector3 startControlLeft = startPosition + n0 * pathWidth / 2;
            Vector3 startcontrolRight = startPosition - n0 * pathWidth / 2;

            Vector3 endControlLeft = endPosition - n1 * pathWidth / 2;
            Vector3 endControlRight = endPosition + n1 * pathWidth / 2;

            // Populate start Node
            PopulateStartNodeMeshData(meshData, startPosition, startLeft, startControlLeft, startRight, startcontrolRight);

            // Actual path
            PopulatePathMeshData(meshData, startLeft, endLeft, startRight, endRight, controlLeft, controlRight);

            // Populate end nodePrefab
            PopulateEndNodeMeshData(meshData, endPosition, endLeft, endControlLeft, endRight, endControlRight);

            MeshUtilities.PopulateMeshTriangles(meshData);
            MeshUtilities.PopulateMeshUvs(meshData);

            return meshData;
        }

        private void PopulateStartNodeMeshData(MeshData meshData, Vector3 startPosition, Vector3 startLeft, Vector3 startControlLeft, Vector3 startRight, Vector3 startcontrolRight) {
            float t;
            for (int i = 0; i < resolution; i++) {
                t = i / (float)(resolution - 1);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startPosition, startLeft, startControlLeft, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startPosition, startRight, startcontrolRight, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(startNodePosition);
                meshData.AddVertice(rightPathVertice);
            }
        }

        private void PopulatePathMeshData(MeshData meshData, Vector3 startLeft, Vector3 endLeft, Vector3 startRight, Vector3 endRight, Vector3 controlLeft, Vector3 controlRight) {
            float t;
            for (int i = 0; i < resolution; i++) {
                t = i / (float)(resolution - 1);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(startLeft, endRight, controlLeft, t);
                Vector3 centerPathVertice = Bezier.QuadraticCurve(startNodePosition, endNodePosition, controlPosition, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(startRight, endLeft, controlRight, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(centerPathVertice);
                meshData.AddVertice(rightPathVertice);
            }
        }

        private void PopulateEndNodeMeshData(MeshData meshData, Vector3 endPosition, Vector3 endLeft, Vector3 endControlLeft, Vector3 endRight, Vector3 endControlRight) {
            float t;
            for (int i = 0; i < resolution; i++) {
                t = i / (float)(resolution - 1);
                Vector3 leftPathVertice = Bezier.QuadraticCurve(endRight, endPosition, endControlRight, t);
                Vector3 rightPathVertice = Bezier.QuadraticCurve(endLeft, endPosition, endControlLeft, t);

                meshData.AddVertice(leftPathVertice);
                meshData.AddVertice(endNodePosition);
                meshData.AddVertice(rightPathVertice);
            }
        }

        public Vector3 GetPathLeftSideVertice(float pathWidth, Vector3 centerVertice, Vector3 startVertice)
        {
            Vector3 verticeDirection = (centerVertice - startVertice).normalized;
            Vector3 left = new(-verticeDirection.z, verticeDirection.y, verticeDirection.x);
            Vector3 leftSideVertice = centerVertice + .5f * pathWidth * left;
            return leftSideVertice;
        }
        public Vector3 GetPathRightSideVertice(float pathWidth, Vector3 centerVertice, Vector3 startVertice)
        {
            Vector3 verticeDirection = (centerVertice - startVertice).normalized;
            Vector3 left = new(-verticeDirection.z, verticeDirection.y, verticeDirection.x);
            Vector3 leftSideVertice = centerVertice - .5f * pathWidth * left;
            return leftSideVertice;
        }
    }
}
