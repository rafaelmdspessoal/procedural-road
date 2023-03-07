﻿using System.Collections;
using UnityEngine;

namespace Road.Mesh.NodeVertices {
    public class CalculateNodeWSIMeshData : MonoBehaviour {
        public static MeshData PopulateStartNode(MeshData meshData, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int roadWidth, int resolution) {
            resolution *= 3;
            float t;
            Vector3 startLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startPosition, controlPosition);
            Vector3 endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endPosition, controlPosition);
            Vector3 controlLeft;

            Vector3 startRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startPosition, controlPosition);
            Vector3 endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, endPosition, controlPosition);
            Vector3 controlRight;

            Vector3 n0 = (startLeft - startPosition).normalized;
            Vector3 n1 = (endRight - endPosition).normalized;

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

            for (int i = resolution / 2 - 1; i < resolution; i++) {
                t = i / (float)(resolution - 2);
                Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endRight, controlLeft, t);
                Vector3 centerRoadVertice = Bezier.QuadraticCurve(startPosition, endPosition, controlPosition, t);
                Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endLeft, controlRight, t);

                meshData.AddVertice(leftRoadVertice);
                meshData.AddVertice(centerRoadVertice);
                meshData.AddVertice(rightRoadVertice);
            }
            return meshData;
        }


        public static MeshData PopulateEndNode(MeshData meshData, Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, int roadWidth, int resolution) {
            resolution *= 3;
            float t;
            Vector3 startLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, startPosition, controlPosition);
            Vector3 endLeft = RoadUtilities.GetRoadLeftSideVertice(roadWidth, endPosition, controlPosition);
            Vector3 controlLeft;

            Vector3 startRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, startPosition, controlPosition);
            Vector3 endRight = RoadUtilities.GetRoadRightSideVertice(roadWidth, endPosition, controlPosition);
            Vector3 controlRight;

            Vector3 n0 = (startLeft - startPosition).normalized;
            Vector3 n1 = (endRight - endPosition).normalized;

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

            for (int i = 0; i < resolution / 2; i++) {
                t = i / (float)(resolution - 2);
                Vector3 leftRoadVertice = Bezier.QuadraticCurve(startLeft, endRight, controlLeft, t);
                Vector3 centerRoadVertice = Bezier.QuadraticCurve(startPosition, endPosition, controlPosition, t);
                Vector3 rightRoadVertice = Bezier.QuadraticCurve(startRight, endLeft, controlRight, t);

                meshData.AddVertice(leftRoadVertice);
                meshData.AddVertice(centerRoadVertice);
                meshData.AddVertice(rightRoadVertice);
            }
            return meshData;
        }
    }
}