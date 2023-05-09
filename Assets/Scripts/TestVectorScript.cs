using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rafael.Utils;
using System.Linq;

public class TestVectorScript : MonoBehaviour
{
    GameObject point_1;
    GameObject point_2;
    GameObject point_3;
    GameObject intersection_1;
    GameObject intersection_2;

    Vector3 intersection_1Pos;
    Vector3 intersection_2Pos;


    // Start is called before the first frame update
    void Start()
    {
        point_1 = RafaelUtils.CreateSphere(Vector3.zero, "point_1", transform, 1);
        point_2 = RafaelUtils.CreateSphere(Vector3.zero, "point_2", transform, 1);
       // point_3 = RafaelUtils.CreateSphere(Vector3.zero, "point_3", transform, 1);

        intersection_1 = RafaelUtils.CreateSphere(Vector3.zero, "intersection_1", transform, 1);
        //intersection_2 = RafaelUtils.CreateSphere(Vector3.zero, "intersection_2", transform, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (RafaelUtils.LineLineIntersection(
            out intersection_1Pos,
            point_1.transform.position,
            point_1.transform.forward,
            point_2.transform.position,
            point_2.transform.forward))
        {
            intersection_1.transform.position = intersection_1Pos;
        }

        //if (RafaelUtils.LineLineIntersection(
        //    out intersection_2Pos,
        //    point_1.transform.position,
        //    point_1.transform.forward,
        //    point_3.transform.position,
        //    point_3.transform.forward))
        //{
        //    intersection_2.transform.position = intersection_2Pos;
        //}
    }

    private void OnDrawGizmos()
    {
        return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(point_1.transform.position, point_1.transform.position + point_1.transform.forward);
        Gizmos.DrawLine(point_2.transform.position, point_2.transform.position + point_2.transform.forward);
        //Gizmos.DrawLine(point_3.transform.position, point_3.transform.position + point_3.transform.forward);
    }
}
