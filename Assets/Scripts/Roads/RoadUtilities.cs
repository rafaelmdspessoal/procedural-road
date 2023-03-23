using Road.Obj;
using Road.NodeObj;
using UnityEngine;
using Road.Placement;

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

        public static Vector3 GetHitPosition(Vector3 hitPosition, GameObject hitObject, bool splitRoad = false) {
            Vector3 targetPosition = hitPosition;
            if (hitObject.TryGetComponent(out Node _)) {
                return hitObject.transform.position;
            } else if (hitObject.TryGetComponent(out RoadObject roadObject)) {
                targetPosition = Bezier.GetClosestPointTo(roadObject, hitPosition);
                if (splitRoad)
                    RoadPlacementManager.Instance.AddRoadToSplit(targetPosition, roadObject);
                return targetPosition;
            }
            return new Vector3(
                targetPosition.x,
                targetPosition.y + 0.1f,
                targetPosition.z
            );
        }

        public static Vector3 GetHitPositionWithSnapping(Vector3 hitPosition, Node hitNode, float angleSnap) {
            Vector3 currentDirection = hitPosition - hitNode.Position;
            Vector3 targetPosition = hitPosition;
            foreach (RoadObject roadObject in hitNode.ConnectedRoads()) {
                Vector3 roadDirection = hitNode.Position - roadObject.ControlNodeObject.transform.position;
                float angle = Vector3.Angle(currentDirection, roadDirection);
                if (angle > angleSnap) continue;

                Vector3 projection = currentDirection.magnitude * Mathf.Cos(angle * Mathf.Deg2Rad) * roadDirection.normalized;
                targetPosition = projection + hitNode.transform.position;
            }
            return targetPosition;
        }
    }
}