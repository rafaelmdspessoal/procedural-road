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
            if (hitObject.TryGetComponent(out Node _)) 
                return hitObject.transform.position;

            if (hitObject.TryGetComponent(out RoadObject roadObject)) {
                targetPosition = Bezier.GetClosestPointTo(roadObject, hitPosition);
                if (splitRoad)
                    RoadPlacementManager.Instance.AddRoadToSplit(targetPosition, roadObject);
                return targetPosition;
            }

            return new Vector3(targetPosition.x, targetPosition.y + 0.1f, targetPosition.z);
        }

        public static Vector3 GetHitPositionWithSnapping(Vector3 hitPosition, Node startNode, int angleSnap) {
            Vector3 currentDirection = hitPosition - startNode.Position;
            Vector3 targetPosition;
            Vector3 baseDirection = Vector3.forward;
            Vector3 projection = SnapTo(currentDirection, baseDirection, angleSnap);
            foreach (RoadObject roadObject in startNode.ConnectedRoads()) {
                baseDirection = (startNode.Position - roadObject.ControlPosition).normalized;
                projection =  SnapTo(currentDirection, baseDirection, angleSnap);
            }

            targetPosition = projection + startNode.Position;
            return targetPosition;
        }

        public static Vector3 GetHitPositionWithSnapping(Vector3 hitPosition, Vector3 startPosition, Vector3 controlPosition, int angleSnap) {
            Vector3 currentDirection = hitPosition - controlPosition;
            Vector3 targetPosition;
            Vector3 baseDirection = (controlPosition - startPosition).normalized;
            Vector3 projection = SnapTo(currentDirection, baseDirection, angleSnap);

            targetPosition = projection + controlPosition;
            return targetPosition;
        }

        private static Vector3 SnapTo(Vector3 v3, Vector3 target, float snapAngle) {
            float angle = Vector3.Angle(v3, target);
            if (angle < snapAngle / 2.0f)          // Cannot do cross product 
                return target * v3.magnitude;  //   with angles 0 & 180
            if (angle > 180.0f - snapAngle / 2.0f)
                return -1 * v3.magnitude * target;

            float t = Mathf.Round(angle / snapAngle);
            float deltaAngle = (t * snapAngle) - angle;

            Vector3 axis = Vector3.Cross(target, v3);
            Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
            return q * v3;
        }
    }
}
