using Path.Entities;
using UnityEngine;

namespace Global.Utils
{
    public static class Bezier
    {
        public static Vector3 QuadraticCurve(Vector3 startPosition, Vector3 endPosition, Vector3 controlPointPosition, float t)
        {
            Vector3 startControlLerp = Lerp(startPosition, controlPointPosition, t);
            Vector3 controlEndLerp = Lerp(controlPointPosition, endPosition, t);
            return Lerp(startControlLerp, controlEndLerp, t);
        }

        public static Vector3 Lerp(Vector3 start, Vector3 end, float t)
        {
            return start + (end - start) * t;
        }

        public static Vector3 GetClosestPointTo(PathObject pathObject, Vector3 position)
        {
            Vector3 point = GetClosestPointTo(
                pathObject.StartNode.Position,
                pathObject.EndNode.Position,
                pathObject.ControlPosition,
                position);
            return point;
        }
        public static Vector3 GetClosestPointTo(Vector3 startPostiion, Vector3 endPosition, Vector3 controlPosition, Vector3 position)
        {
            float t = 0;
            float minDistanceToSegment = Mathf.Infinity;
            Vector3 point = Vector3.positiveInfinity;
            while (t <= 1)
            {
                t += .001f;
                point = QuadraticCurve(
                    startPostiion,
                    endPosition,
                    controlPosition,
                    t
                );
                float distance = Vector3.Distance(position, point);
                if (distance < minDistanceToSegment)
                    minDistanceToSegment = distance;
                else
                    break;
            }
            if (point == Vector3.positiveInfinity)
                Debug.LogError("closest point is infinity");
            return point;
        }

        public static float GetLengh(Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition)
        {
            float distance = 0;
            float t = 0f;
            Vector3 point_2 = startPosition;
            while (t <= 1)
            {
                t += .001f;
                Vector3 point_1 = QuadraticCurve(
                    startPosition,
                    endPosition,
                    controlPosition,
                    t
                );
                distance += Vector3.Distance(point_1, point_2);
                point_2 = point_1;
            }
            return distance;
        }

        public static Vector3 GetTangentAt(PathObject pathObject, Vector3 point, out Vector3 pointA, out Vector3 pointB)
        {
            float t = 0;
            float minDistanceToSegment = Mathf.Infinity;
            Vector3 tangent = Vector3.positiveInfinity;

            Vector3 startPosition = pathObject.StartNode.transform.position;
            Vector3 endPosition = pathObject.EndNode.transform.position;
            Vector3 controlPointPosition = pathObject.ControlPosition;

            pointA = Vector3.negativeInfinity;
            pointB = Vector3.negativeInfinity;

            while (t <= 1)
            {
                t += .001f;
                Vector3 bezierPoint = QuadraticCurve(
                    startPosition,
                    endPosition,
                    controlPointPosition,
                    t
                );
                float distance = Vector3.Distance(bezierPoint, point);
                if (distance < minDistanceToSegment)
                    minDistanceToSegment = distance;
                else
                {
                    pointA = Lerp(startPosition, controlPointPosition, t);
                    pointB = Lerp(controlPointPosition, endPosition, t);
                    tangent = pointB - pointA;
                    break;
                }
            }
            if (tangent == Vector3.positiveInfinity)
                Debug.LogError("tangent point is infinity");
            if (pointA == Vector3.positiveInfinity)
                Debug.LogError("pointA point is infinity");
            if (pointB == Vector3.positiveInfinity)
                Debug.LogError("pointB point is infinity");
            return tangent.normalized;
        }

        public static Vector3 GetOffsettedPosition(Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, float offsetDistance)
        {
            float t = 0;
            Vector3 offsetPosition = Vector3.positiveInfinity;

            while (t <= 1)
            {
                t += .001f;
                Vector3 bezierPoint = QuadraticCurve(
                    startPosition,
                    endPosition,
                    controlPosition,
                    t
                );
                if ((bezierPoint - startPosition).magnitude >= offsetDistance)
                {
                    offsetPosition = bezierPoint;
                    return offsetPosition;
                }
            }

            if (offsetPosition == Vector3.positiveInfinity)
                Debug.LogError("offsetPosition is infinity");

            return offsetPosition;
        }

        public static OrientedPoint GetBezierOP(
           Transform startNode,
           Vector3 startControlNode,
           Transform endNode,
           Vector3 endControlNode,
           float t)
        {
            Vector3 p0 = startNode.position;
            Vector3 p1 = startControlNode;
            Vector3 p2 = endControlNode;
            Vector3 p3 = endNode.position;

            Vector3 a = Vector3.Lerp(p0, p1, t);
            Vector3 b = Vector3.Lerp(p1, p2, t);
            Vector3 c = Vector3.Lerp(p2, p3, t);

            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);

            Vector3 position = Vector3.Lerp(d, e, t);

            Vector3 tangent = (e - d).normalized;

            Vector3 up = Vector3.Lerp(startNode.up, endNode.up, t).normalized;
            Quaternion orientation = Quaternion.LookRotation(tangent, up);

            return new OrientedPoint(position, orientation);
        }

        public static float GetAproxLength(OrientedPoint[] path)
        {
            float dist = 0;
            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector3 a = path[i].position;
                Vector3 b = path[i + 1].position;
                dist += Vector3.Distance(a, b);
            }

            return dist;
        }
    }
}