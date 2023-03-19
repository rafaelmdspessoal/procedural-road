using System.Collections;
using UnityEngine;

namespace Road.Utilities {

    public static class RoadUtilities {

        public static Vector3 GetRoadLeftSideVertice(float roadWidth, Vector3 centerVertice, Vector3 startVertice) {
            Vector3 verticeDirection = GetVerticeNormalizedDirection(startVertice, centerVertice);
            Vector3 left = new(-verticeDirection.z, verticeDirection.y, verticeDirection.x);
            Vector3 leftSideVertice = centerVertice + .5f * roadWidth * left;
            return leftSideVertice;
        }

        public static Vector3 GetRoadRightSideVertice(float roadWidth, Vector3 centerVertice, Vector3 startVertice) {
            Vector3 verticeDirection = GetVerticeNormalizedDirection(startVertice, centerVertice);
            Vector3 right = new Vector3(-verticeDirection.z, verticeDirection.y, verticeDirection.x) * -1f;
            Vector3 rightSideVertice = centerVertice + .5f * roadWidth * right;
            return rightSideVertice;
        }

        private static Vector3 GetVerticeNormalizedDirection(Vector3 controlPosition, Vector3 verticePosition) {
            return (verticePosition - controlPosition).normalized;
        }

        public static GameObject CreateNodeGFX(RoadObjectSO roadObjectSO) {
            GameObject nodeGFX = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nodeGFX.transform.GetComponent<Collider>().enabled = false;
            nodeGFX.transform.localScale = roadObjectSO.roadWidth * Vector3.one;
            nodeGFX.transform.name = "Node GFX";
            return nodeGFX;
        }

        public static GameObject CreateControlNode(RoadObjectSO roadObjectSO, Vector3 controlNodePosition) {
            GameObject controlNodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            controlNodeObject.transform.localScale = 0.25f * roadObjectSO.roadWidth * Vector3.one;
            controlNodeObject.transform.position = controlNodePosition;
            controlNodeObject.transform.name = "Control Node";
            return controlNodeObject;
        }
    }
}