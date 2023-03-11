using UnityEngine;
using MeshHandler.Utilities;

namespace MeshHandler.Road.Temp.Visual {

    public class CalculateRoadTempMesh {

        private Vector3 startNodePosition;
        private Vector3 endNodePosition;
        private Vector3 controlPosition;

        private readonly int resolution;
        private readonly int roadWidth;

        public CalculateRoadTempMesh(Vector3 startNodePosition, Vector3 endNodePosition, Vector3 controlPosition, int roadWidth, int resolution) {
            this.startNodePosition = startNodePosition;
            this.endNodePosition = endNodePosition;
            this.controlPosition = controlPosition;
            this.roadWidth = roadWidth;
            this.resolution = resolution;
        }

        public MeshData PopulateRoadMeshVertices(MeshData meshData) {

            Vector3 startPosition = startNodePosition + (startNodePosition - controlPosition).normalized * roadWidth / 2;
            Vector3 endPosition = endNodePosition + (endNodePosition - controlPosition).normalized * roadWidth / 2;

            Vector3 startLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startNodePosition, controlPosition);
            Vector3 endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endNodePosition, controlPosition);
            Vector3 startControlLeft;
            Vector3 endControlLeft;

            Vector3 startRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startNodePosition, controlPosition);
            Vector3 endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, endNodePosition, controlPosition);

            Vector3 startcontrolRight;
            Vector3 endControlRight;

            Vector3 controlLeft;
            Vector3 controlRight;

            Vector3 n0 = (startLeft - startNodePosition).normalized;
            Vector3 n1 = (endRight - endNodePosition).normalized;

            if (Vector3.Angle(n0, n1) != 0) {
                // Road is NOT straight, so the DOT product is not 0!
                // This fails for angles > 90, so we must deal with it later
                controlLeft = controlPosition + ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
                controlRight = controlPosition - ((n0 + n1) * roadWidth) / Vector3.Dot((n0 + n1), (n0 + n1));
            } else {
                // Road is traight, so calculations are easier
                controlLeft = controlPosition + n0 * roadWidth / 2;
                controlRight = controlPosition - n1 * roadWidth / 2;
            }

            // Road is traight, so calculations are easier
            startControlLeft = startPosition + n0 * roadWidth / 2;
            startcontrolRight = startPosition - n1 * roadWidth / 2;

            endControlLeft = endPosition - n0 * roadWidth / 2;
            endControlRight = endPosition + n1 * roadWidth / 2;

            // Populate start Node
            PopulateStartNodeMeshData(meshData, startPosition, startLeft, startControlLeft, startRight, startcontrolRight);

            // Actual road
            PopulateRoadMeshData(meshData, startLeft, endLeft, startRight, endRight, controlLeft, controlRight);

            // Populate end node
            PopulateEndNodeMeshData(meshData, endPosition, endLeft, endControlLeft, endRight, endControlRight);

            MeshUtilities.PopulateMeshTriangles(meshData);
            MeshUtilities.PopulateMeshUvs(meshData);

            return meshData;
        }

        private void PopulateStartNodeMeshData(MeshData meshData, Vector3 startPosition, Vector3 startLeft, Vector3 startControlLeft, Vector3 startRight, Vector3 startcontrolRight) {
            float t;
            for (int i = 0; i < resolution; i++) {
                t = i / (float)(resolution - 1);
                Vector3 leftRoadVertice = Bezier.QuadraticCurve(startPosition, startLeft, startControlLeft, t);
                Vector3 rightRoadVertice = Bezier.QuadraticCurve(startPosition, startRight, startcontrolRight, t);

                meshData.AddVertice(leftRoadVertice);
                meshData.AddVertice(startNodePosition);
                meshData.AddVertice(rightRoadVertice);
            }
        }

        private void PopulateRoadMeshData(MeshData meshData, Vector3 startLeft, Vector3 endLeft, Vector3 startRight, Vector3 endRight, Vector3 controlLeft, Vector3 controlRight) {
            float t;
            for (int i = 0; i < resolution; i++) {
                t = i / (float)(resolution - 1);
                Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endRight, controlLeft, t);
                Vector3 centerRoadVertice = Bezier.QuadraticCurve(startNodePosition, endNodePosition, controlPosition, t);
                Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endLeft, controlRight, t);

                meshData.AddVertice(leftRoadVertice);
                meshData.AddVertice(centerRoadVertice);
                meshData.AddVertice(rightRoadVertice);
            }
        }

        private void PopulateEndNodeMeshData(MeshData meshData, Vector3 endPosition, Vector3 endLeft, Vector3 endControlLeft, Vector3 endRight, Vector3 endControlRight) {
            float t;
            for (int i = 0; i < resolution; i++) {
                t = i / (float)(resolution - 1);
                Vector3 leftRoadVertice = Bezier.QuadraticCurve(endRight, endPosition, endControlRight, t);
                Vector3 rightRoadVertice = Bezier.QuadraticCurve(endLeft, endPosition, endControlLeft, t);

                meshData.AddVertice(leftRoadVertice);
                meshData.AddVertice(endNodePosition);
                meshData.AddVertice(rightRoadVertice);
            }
        }
    }
}
