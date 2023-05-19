﻿using UnityEngine;
using UnityEngine.InputSystem;
using World;
using Path.Entities;
using Path.Entities.SO;
using Path.PlacementSystem;

namespace Path.Utilities {

    public static class PathUtilities {       
        public static Vector3 GetPathLeftSideVertice(float pathWidth, Vector3 centerVertice, Vector3 startVertice) {
            Vector3 verticeDirection = (centerVertice - startVertice).normalized;
            Vector3 left = new(-verticeDirection.z, verticeDirection.y, verticeDirection.x);
            Vector3 leftSideVertice = centerVertice + .5f * pathWidth * left;
            return leftSideVertice;
        }
        public static Vector3 GetLeftPointTo(Vector3 point, Vector3 direction, int distance)
        {
            Vector3 left = new(-direction.z, direction.y, direction.x);
            Vector3 leftSideVertice = point + distance * left;
            return leftSideVertice;
        }

        public static Vector3 GetPathRightSideVertice(float pathWidth, Vector3 centerVertice, Vector3 startVertice)
        {
            Vector3 verticeDirection = (centerVertice - startVertice).normalized;
            Vector3 left = new(-verticeDirection.z, verticeDirection.y, verticeDirection.x);
            Vector3 leftSideVertice = centerVertice - .5f * pathWidth * left;
            return leftSideVertice;
        }

        public static Vector3 GetRightPointTo(Vector3 point, Vector3 direction, int distance)
        {
            Vector3 left = new(-direction.z, direction.y, direction.x);
            Vector3 leftSideVertice = point - distance * left;
            return leftSideVertice;
        }

        public static GameObject CreateNodeGFX(PathSO pathObjectSO) {
            GameObject nodeGFX = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nodeGFX.layer = 2;
            nodeGFX.transform.GetComponent<SphereCollider>().radius = 1f;
            nodeGFX.transform.localScale = pathObjectSO.width * Vector3.one;
            nodeGFX.transform.name = "Node GFX";
            return nodeGFX;
        }

        public static GameObject CreateControlNode(PathSO pathObjectSO, Vector3 controlNodePosition) {
            GameObject controlNodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            controlNodeObject.transform.localScale = 0.25f * pathObjectSO.width * Vector3.one;
            controlNodeObject.transform.position = controlNodePosition;
            controlNodeObject.transform.name = "Control Node";
            return controlNodeObject;
        }

        public static Vector3 GetHitPositionWithSnapping(Vector3 hitPosition, NodeObject startNode, int angleSnap) {
            Vector3 currentDirection = hitPosition - startNode.Position;
            Vector3 targetPosition;
            Vector3 baseDirection = Vector3.forward;
            Vector3 projection = SnapTo(currentDirection, baseDirection, angleSnap);
            foreach (PathObject pathObject in startNode.ConnectedPaths) {
                baseDirection = (startNode.Position - pathObject.ControlPosition).normalized;
                projection =  SnapTo(currentDirection, baseDirection, angleSnap);
            }

            targetPosition = projection + startNode.Position;
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

        public static bool TryRaycastObject(out Vector3 hitPosition, out GameObject hitObject, int radius = 4, bool splitPath = false)
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

                    if (hitObj.TryGetComponent(out NodeObject _))
                    {
                        hitObject = hitObj;
                        hitPosition = hitObj.transform.position;
                        return true;
                    }
                }
                foreach (RaycastHit sphereHit in sphereHits)
                {
                    GameObject hitObj = sphereHit.transform.gameObject;
                    if (hitObj.TryGetComponent(out PathObject pathObject))
                    {
                        hitObject = hitObj;
                        hitPosition = Bezier.GetClosestPointTo(pathObject, hitPosition);
                        if (splitPath)
                            PathPlacementSystem.Instance.AddPathToSplit(hitPosition, pathObject);
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