using UnityEngine.InputSystem;
using UnityEngine;

namespace Rafael.Utils
{
    public static class RafaelUtils
    {
        public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1,
            Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {

            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //is coplanar, and not parallel
            //if (Mathf.Abs(planarFactor) < 0.01f)
            //{
            //    intersection = (linePoint1 + linePoint2) / 2;
            //    return false;
            //}

            if (crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = (linePoint1 + (lineVec1 * s));// N�o sei pq isso n�o funcionou + linePoint2 + (lineVec2 * s)) / 2;
                return true;
            }
            else
            {
                intersection = (linePoint1 + linePoint2) / 2;
                return false;
            }
        }

        public static bool TryRaycastObject(out RaycastHit hit) {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                return true;

            return false;
        }

        public static GameObject CreateSphere(Vector3 position, string name, Transform parent = null, float scale = .25f) {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = scale * Vector3.one;
            sphere.transform.position = position;
            sphere.transform.name = name;
            sphere.transform.parent = parent;
            return sphere;
        }
    }

}