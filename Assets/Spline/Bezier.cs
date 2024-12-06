using Path.Entities;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Spline.Utils
{
    public static class Bezier
    {
        public static OrientedPoint GetOP(
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

        public static OrientedPoint GetOP(
           Vector3 startNodePosition,
           Vector3 startNodeUp,
           Vector3 startControlNode,
           Vector3 endNodePosition,
           Vector3 endNodeUp,
           Vector3 endControlNode,
           float t)
        {
            Vector3 p0 = startNodePosition;
            Vector3 p1 = startControlNode;
            Vector3 p2 = endControlNode;
            Vector3 p3 = endNodePosition;

            Vector3 a = Vector3.Lerp(p0, p1, t);
            Vector3 b = Vector3.Lerp(p1, p2, t);
            Vector3 c = Vector3.Lerp(p2, p3, t);

            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);

            Vector3 position = Vector3.Lerp(d, e, t);

            Vector3 tangent = (e - d).normalized;

            Vector3 up = Vector3.Lerp(startNodeUp, endNodeUp, t).normalized;
            Quaternion orientation = Quaternion.LookRotation(tangent, up);

            return new OrientedPoint(position, orientation);
        }

        public static Vector3 GetPoint(
           Vector3 startPosition,
           Vector3 startControlPosition,
           Vector3 endPosition,
           Vector3 endControlPosition,
           float t)
        {
            Vector3 a = Vector3.Lerp(startPosition, startControlPosition, t);
            Vector3 b = Vector3.Lerp(startControlPosition, endControlPosition, t);
            Vector3 c = Vector3.Lerp(endControlPosition, endPosition, t);

            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);

            return Vector3.Lerp(d, e, t);
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

        public static float GetAproxLength(Vector3[] path)
        {
            float dist = 0;
            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector3 a = path[i];
                Vector3 b = path[i + 1];
                dist += Vector3.Distance(a, b);
            }

            return dist;
        }

        public static OrientedPoint[] CalculateSplineOP(
            Transform startNodeTransform,
            Vector3 startControlNodePosition,
            Vector3 endControlNodePosition,
            Transform endNodeTransform,
            int resolution)
        {
            OrientedPoint[] path = new OrientedPoint[resolution];
            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                path[i] = GetOP(
                    startNodeTransform,
                    startControlNodePosition,
                    endNodeTransform,
                    endControlNodePosition,
                    t);
            }
            return path;
        }

        public static OrientedPoint[] CalculateSplineOP(
            Vector3 startNodePosition,
            Vector3 startNodeUp,
            Vector3 startControlNodePosition,
            Vector3 endControlNodePosition,
            Vector3 endNodePosition,
            Vector3 endNodeUp,
            int resolution)
        {
            OrientedPoint[] path = new OrientedPoint[resolution];
            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                path[i] = GetOP(
                    startNodePosition,
                    startNodeUp,
                    startControlNodePosition,
                    endNodePosition,
                    endNodeUp,
                    endControlNodePosition,
                    t);
            }
            return path;
        }


        public static Vector3[] CalculateSplinePoints(
            Vector3 startPosition,
            Vector3 controlPosition,
            Vector3 endPosition,
            int resolution,
            float ratio)
        {
            Vector3[] path = new Vector3[resolution];

            PositionControlNodes(
                ratio,
                startPosition,
                endPosition,
                controlPosition,
                out Vector3 startControlPosition,
                out Vector3 endControlPosition);

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                path[i] = GetPoint(
                    startPosition,
                    startControlPosition,
                    endPosition,
                    endControlPosition,
                    t);
            }
            return path;
        }
        public static void PositionControlNodes(
            float ratio,
            Vector3 startNodePosition,
            Vector3 endNodePosition,
            Vector3 controlNodePosition,
            out Vector3 startControlNodePosition,
            out Vector3 endControlNodePosition)
        {
            Vector3 startControlNodeOrientation = (controlNodePosition - startNodePosition);
            Vector3 endControlNodeOrientation = (controlNodePosition - endNodePosition);

            ratio = Mathf.Clamp(ratio, 0.55f, 1);

            startControlNodePosition = (
                ratio *
                startControlNodeOrientation.normalized *
                startControlNodeOrientation.magnitude) +
                startNodePosition;

            endControlNodePosition = (
                ratio *
                endControlNodeOrientation.normalized *
                endControlNodeOrientation.magnitude) +
                endNodePosition;
        }

    }
}