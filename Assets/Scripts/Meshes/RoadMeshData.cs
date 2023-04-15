using UnityEngine;
using Roads.Utilities;
using Nodes;
using UnityEditor;

namespace Roads.MeshHandler.Data {
    public class RoadMeshData {

        private readonly RoadObject roadObject;

        private readonly int resolution;
        private readonly int roadWidth;

        /// <summary>
        /// Generates the geometry for the given Road Object
        /// </summary>
        /// <param name="roadObject"></param>
        public RoadMeshData(RoadObject roadObject) {
            this.roadObject = roadObject;

            roadWidth = roadObject.RoadWidth;
            resolution = roadObject.RoadResolution;
        }

        /// <summary>
        /// Populate mesh for given Road Object
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public MeshData PopulateRoadMeshVertices(MeshData meshData) 
        {
            Vector3 startCenterNode = roadObject.StartMeshCenterPosition();
            Vector3 endCenterNode = roadObject.EndMeshCenterPosition();

            Vector3 startLeft = roadObject.StartMeshLeftPosition();
            Vector3 startRight = roadObject.StartMeshRightPosition();

            Vector3 endLeft = roadObject.EndMeshLeftPosition();
            Vector3 endRight = roadObject.EndMeshRightPosition();

            Vector3 controlPosition = roadObject.ControlPosition();

            Vector3 controlLeft;
            Vector3 controlRight;

            Vector3 n0 = (startLeft - startCenterNode).normalized;
            Vector3 n1 = (endRight - endCenterNode).normalized;

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

            meshData = MeshUtilities.PopulateRoadMeshVertices(
                meshData, 
                resolution,
                startLeft,
                endLeft,
                controlLeft,
                startCenterNode,
                endCenterNode,
                controlPosition,
                startRight,
                endRight,
                controlRight);

            return meshData;
        }
    }
}