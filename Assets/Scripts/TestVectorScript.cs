using System.Collections.Generic;
using UnityEngine;
using Rafael.Utils;

public class TestVectorScript : MonoBehaviour
{
    GameObject point_1;
    GameObject point_2;
    GameObject point_3;
    GameObject intersection_1;
    GameObject intersection_2;

    Vector3 intersection_1Pos;
    Vector3 intersection_2Pos;

    public int resolution = 20;
    public int width = 6;


    // Start is called before the first frame update
    void Start()
    {
        point_1 = RafaelUtils.CreateSphere(Vector3.left * 2, "start", transform, .1f);
        point_2 = RafaelUtils.CreateSphere(new Vector3(0,-2,0), "control", transform, .1f);
        point_3 = RafaelUtils.CreateSphere(Vector3.right * 2, "end", transform, .1f);
    }

    // Update is called once per frame
    void Update()
    {
        //if (RafaelUtils.LineLineIntersection(
        //    out intersection_1Pos,
        //    point_1.transform.position,
        //    point_1.transform.forward,
        //    point_2.transform.position,
        //    point_2.transform.forward))
        //{
        //    intersection_1.transform.position = intersection_1Pos;
        //}

        List<Vector3> points = new();
        List<Vector3> points2 = new();
        List<Vector3> tangents = new();
        Vector3 startPosition = point_1.transform.position;
        Vector3 controlPointPosition = point_2.transform.position;
        Vector3 endPosition = point_3.transform.position;
        Vector3 prevTangent = Vector3.zero;
        Vector3 prevPoint = Vector3.zero; ;

        float t;

        for (int i = 0; i < resolution; i++)
        {
            t = i / (float)(resolution - 1);

            Vector3 point = Bezier.QuadraticCurve(
                startPosition,
                endPosition ,
                controlPointPosition ,
                t);
            Vector3 pointA = Bezier.Lerp(startPosition, controlPointPosition, t);
            Vector3 pointB = Bezier.Lerp(controlPointPosition, endPosition, t);
            Vector3 tangent = pointB - pointA;
            if (i > 0 && i < resolution - 1)
            {
                float angle = Vector3.Angle(tangent, prevTangent) * Mathf.Deg2Rad;
                float dist = (point - prevPoint).magnitude;

                if (dist / width <= angle)
                {
                    print(angle * Mathf.Rad2Deg);
                    continue;
                }
            }
            prevTangent = tangent;
            prevPoint = point;

            tangents.Add(tangent);
            points.Add(point);
        }
        for (int i = 0; i < resolution; i++)
        {
            t = i / (float)(resolution - 1);

            Vector3 point = Bezier.QuadraticCurve(
                startPosition,
                endPosition,
                controlPointPosition,
                t);
            points2.Add(point);
        }
        for (int i = 0; i < points.Count - 1; i++)
        {
            Debug.DrawLine(points[i], points[i + 1], Color.black);
        }
        for (int i = 0; i < points2.Count - 1; i++)
        {
            Debug.DrawLine(points2[i], points2[i + 1], Color.magenta);
        }
        for (int i = 0; i < tangents.Count; i++)
        {
            Debug.DrawLine(points[i], (tangents[i].normalized * width) + points[i], Color.green);
            Vector3 left = new Vector3(-tangents[i].z, tangents[i].y, tangents[i].x);           
            Debug.DrawLine(points[i], (left.normalized * width) + points[i], Color.blue);
            Debug.DrawLine(points[i], -(left.normalized * width) + points[i], Color.blue);
        }
        for (int i = 0; i < tangents.Count - 1; i++)
        {
            float angle = Vector3.Angle(tangents[i + 1], tangents[i]) * Mathf.Deg2Rad;
            float dist = (points[i + 1] - points[i]).magnitude;

            if (dist / width <= angle)
            {
                print("xuxa");
            }
        }
    }
}
