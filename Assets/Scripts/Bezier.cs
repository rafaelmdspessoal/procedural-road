using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector3 LinearCurve(Vector3 startPos, Vector3 endPos, float t)
    {
        return (1 - t) * startPos + t * endPos;
    }

    public static Vector3 QuadraticCurve(Vector3 startPosition, Vector3 endPosition, Vector3 controlPointPosition, float t)
    {
        Vector3 startControlLerp = Lerp(startPosition, controlPointPosition, t);
        Vector3 controlEndLerp = Lerp(controlPointPosition, endPosition, t);
        return Lerp(startControlLerp, controlEndLerp, t);
    }

    private static Vector3 Lerp(Vector3 start, Vector3 end, float t) {
        return start + (end - start) * t;
    }

    public static Vector3 GetClosestPointTo(RoadObject roadObject, Vector3 position)
    {
        float t = 0;
        float minDistanceToSegment = Mathf.Infinity;
        Vector3 point = Vector3.negativeInfinity;
        while (t <= 1)
        {
            t += .001f;
            point = QuadraticCurve(
                roadObject.StartNode.Position, 
                roadObject.EndNode.Position, 
                roadObject.ControlNodeObject.transform.position, 
                t
            );
            float distance = Vector3.Distance(position, point);
            if (distance <= minDistanceToSegment)
                minDistanceToSegment = distance;
            else
                break;
        }
        return point;
    }

    public static float GetLengh(Vector3 startPos, Vector3 endPos)
    {
        return (endPos - startPos).magnitude;
    }

    public static Vector3 GetTangentAt(RoadObject roadObject, Vector3 point, out Vector3 pointA, out Vector3 pointB)
    {
        float t = 0;
        float minDistanceToSegment = Mathf.Infinity;
        Vector3 tangent = Vector3.negativeInfinity;

        Vector3 startPosition = roadObject.StartNode.transform.position;
        Vector3 endPosition = roadObject.EndNode.transform.position;
        Vector3 controlPointPosition = roadObject.ControlNodeObject.transform.position;

        pointA = Vector3.negativeInfinity;
        pointB = Vector3.negativeInfinity;

        while (t <= 1) {
            t += .001f;
            Vector3 bezierPoint = QuadraticCurve(
                startPosition,
                endPosition,
                controlPointPosition,
                t
            );
            float distance = Vector3.Distance(bezierPoint, point);
            if (distance <= minDistanceToSegment)
                minDistanceToSegment = distance;
            else {
                pointA = Lerp(startPosition, controlPointPosition, t);
                pointB = Lerp(controlPointPosition, endPosition, t);
                tangent = pointB - pointA;
                break;
            }
        }
        return tangent;
    }

    public static Vector3 GetOffsettedPosition(Vector3 startPosition, Vector3 endPosition, Vector3 controlPosition, float offsetDistance)
    {
        float t = 0;
        Vector3 offsetPosition = Vector3.zero;

        while (t <= 1) {
            t += .001f;
            Vector3 bezierPoint = QuadraticCurve(
                startPosition,
                endPosition,
                controlPosition,
                t
            );
            if ((bezierPoint - startPosition).magnitude >= offsetDistance) {
                offsetPosition = bezierPoint;
                return offsetPosition;
            }
        }
        return offsetPosition;
    }
}
