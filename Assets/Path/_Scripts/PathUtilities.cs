using UnityEngine;
using UnityEngine.InputSystem;
using World;
using Path.Entities;
using Path.Entities.SO;
using Path.PlacementSystem;
using System.Drawing.Printing;

namespace Path.Utilities {

    public static class PathUtilities {       
      
        public static Vector3 GetLeftPointTo(Vector3 point, Vector3 direction, int distance)
        {
            Vector3 left = new(-direction.z, direction.y, direction.x);
            Vector3 leftSideVertice = point + distance * left;
            return leftSideVertice;
        }

        public static Vector3 GetRightPointTo(Vector3 point, Vector3 direction, int distance)
        {
            Vector3 left = new(-direction.z, direction.y, direction.x);
            Vector3 leftSideVertice = point - distance * left;
            return leftSideVertice;
        }

        public static GameObject UpdateOrCreateNodeGFX(PathSO pathObjectSO, GameObject existingNodeGFX) 
        {
            GameObject nodeGFX = existingNodeGFX;
            if (nodeGFX == null)
            {
                nodeGFX = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }
            nodeGFX.layer = 2;
            nodeGFX.transform.GetComponent<SphereCollider>().radius = 1f;
            nodeGFX.transform.localScale = pathObjectSO.Width * Vector3.one;
            nodeGFX.transform.name = "Node GFX";
            return nodeGFX;
        }

        public static Vector3 SnapTo(Vector3 current, Vector3 target, float snapAngle) {
            float angle = Vector3.Angle(current, target);
            if (angle < snapAngle / 2.0f)          // Cannot do cross product 
                return target * current.magnitude;  //   with angles 0 & 180
            if (angle > 180.0f - snapAngle / 2.0f)
                return -1 * current.magnitude * target;

            float t = Mathf.Round(angle / snapAngle);
            float deltaAngle = (t * snapAngle) - angle;

            Vector3 axis = Vector3.Cross(target, current);
            Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
            return q * current;
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
    }
}
