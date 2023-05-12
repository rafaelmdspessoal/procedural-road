using UnityEngine;
using Roads.Placement;
using Nodes;
using UnityEngine.InputSystem;
using TMPro;
using World;
using System.Drawing.Printing;

namespace Roads.Utilities {

    public static class RoadUtilities {       
        public static Vector3 GetRoadLeftSideVertice(float roadWidth, Vector3 centerVertice, Vector3 startVertice) {
            Vector3 verticeDirection = (centerVertice - startVertice).normalized;
            Vector3 left = new(-verticeDirection.z, verticeDirection.y, verticeDirection.x);
            Vector3 leftSideVertice = centerVertice + .5f * roadWidth * left;
            return leftSideVertice;
        }
        public static Vector3 GetLeftPointTo(Vector3 point, Vector3 direction, int distance)
        {
            Vector3 left = new(-direction.z, direction.y, direction.x);
            Vector3 leftSideVertice = point + distance * left;
            return leftSideVertice;
        }

        public static Vector3 GetRoadRightSideVertice(float roadWidth, Vector3 centerVertice, Vector3 startVertice)
        {
            Vector3 verticeDirection = (centerVertice - startVertice).normalized;
            Vector3 left = new(-verticeDirection.z, verticeDirection.y, verticeDirection.x);
            Vector3 leftSideVertice = centerVertice - .5f * roadWidth * left;
            return leftSideVertice;
        }

        public static Vector3 GetRightPointTo(Vector3 point, Vector3 direction, int distance)
        {
            Vector3 left = new(-direction.z, direction.y, direction.x);
            Vector3 leftSideVertice = point - distance * left;
            return leftSideVertice;
        }

        private static Vector3 GetVerticeNormalizedDirection(Vector3 controlPosition, Vector3 verticePosition) {
            return (verticePosition - controlPosition).normalized;
        }

        public static GameObject CreateNodeGFX(RoadObjectSO roadObjectSO) {
            GameObject nodeGFX = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nodeGFX.layer = 2;
            nodeGFX.transform.GetComponent<SphereCollider>().radius = 1f;
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

        public static Vector3 GetHitPositionWithSnapping(Vector3 hitPosition, Node startNode, int angleSnap) {
            Vector3 currentDirection = hitPosition - startNode.Position;
            Vector3 targetPosition;
            Vector3 baseDirection = Vector3.forward;
            Vector3 projection = SnapTo(currentDirection, baseDirection, angleSnap);
            foreach (RoadObject roadObject in startNode.ConnectedRoads) {
                baseDirection = (startNode.Position - roadObject.ControlNodePosition).normalized;
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

        public static Vector3 GetProjectedPosition(Vector3 positionToProject, Vector3 directionToProject, Vector3 intersectionPosition) 
        {
            Vector3 currentDirection = positionToProject - intersectionPosition;
            float angle = Vector3.Angle(currentDirection, directionToProject);

            float minProjectionLengh = Mathf.Clamp(
                currentDirection.magnitude * Mathf.Cos(angle * Mathf.Deg2Rad),
                10f,
                Mathf.Infinity);

            Vector3 projectedPosition = minProjectionLengh * directionToProject.normalized;
            return projectedPosition + intersectionPosition;
        }

        public static bool TryRaycastObject(out Vector3 hitPosition, out GameObject hitObject, int radius = 4, bool splitRoad = false)
        {
            hitObject = null;
            hitPosition = Vector3.zero;
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity))
            {
                hitPosition = rayHit.point;
                hitObject = rayHit.transform.gameObject;
                RaycastHit[] sphereHits = Physics.SphereCastAll(hitPosition, radius, new Vector3(1f, 0, 0), radius);
                foreach (RaycastHit sphereHit in sphereHits)
                {
                    GameObject hitObj = sphereHit.transform.gameObject;

                    if (hitObj.TryGetComponent(out Node _))
                    {
                        hitObject = hitObj;
                        hitPosition = hitObj.transform.position;
                        return true;
                    }
                }
                foreach (RaycastHit sphereHit in sphereHits)
                {
                    GameObject hitObj = sphereHit.transform.gameObject;
                    if (hitObj.TryGetComponent(out RoadObject roadObject))
                    {
                        hitObject = hitObj;
                        hitPosition = Bezier.GetClosestPointTo(roadObject, hitPosition);
                        if (splitRoad)
                            RoadPlacementSystem.Instance.AddRoadToSplit(hitPosition, roadObject);
                        return true;
                    }
                }
                if (hitObject.TryGetComponent(out Ground ground))
                    hitPosition = new Vector3(hitPosition.x, hitPosition.y + 0.1f, hitPosition.z);
                
                return true;
            }
            return false;
        }
    }
}
