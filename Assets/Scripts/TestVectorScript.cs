using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rafael.utils;
using System.Linq;

public class TestVectorScript : MonoBehaviour
{
    GameObject center;
    GameObject endMain;
    GameObject endLeft;
    GameObject endMid;
    GameObject endRight;

    // Start is called before the first frame update
    void _Start()
    {
        center = RafaelUtils.CreateSphere(Vector3.zero, "center", 1);
        endMain = RafaelUtils.CreateSphere(Vector3.zero, "end main", 1);
        endLeft = RafaelUtils.CreateSphere(Vector3.zero, "end left", 1);
        endMid = RafaelUtils.CreateSphere(Vector3.zero, "end mid", 1);
        endRight = RafaelUtils.CreateSphere(Vector3.zero, "end right", 1);
    }

    // Update is called once per frame
    void _Update()
    {
        List<float> angles = new List<float>();

        Vector3 centerPos = center.transform.position;
        Vector3 endMainPos = endMain.transform.position;
        Vector3 endLeftPos = endLeft.transform.position;
        Vector3 endMidPos = endMid.transform.position;
        Vector3 endRightPos = endRight.transform.position;

        Vector3 mainRoadDir = GetNormalizedDirection(centerPos, endMainPos);
        Vector3 leftRoadDir = GetNormalizedDirection(centerPos, endLeftPos);
        Vector3 midRoadDir = GetNormalizedDirection(centerPos, endMidPos);
        Vector3 rightRoadDir = GetNormalizedDirection(centerPos, endRightPos);

        float leftAngle = Vector3.SignedAngle(mainRoadDir, leftRoadDir, Vector3.up);
        float midAngle = Vector3.SignedAngle(mainRoadDir, midRoadDir, Vector3.up);
        float rightAngle = Vector3.SignedAngle(mainRoadDir, rightRoadDir, Vector3.up);

        angles.Add(rightAngle);
        angles.Add(midAngle);
        angles.Add(leftAngle);

        Debug.DrawLine(centerPos, endMainPos, Color.blue);
        Debug.DrawLine(centerPos, endLeftPos, Color.cyan);
        Debug.DrawLine(centerPos, endMidPos, Color.green);
        Debug.DrawLine(centerPos, endRightPos, Color.black);

        print("leftAngle: " + leftAngle);
        print("midAngle: " + midAngle);
        print("rightAngle: " + rightAngle);

        PrintAngles(angles);

    }

    void PrintAngles(List<float> angles) {
        angles.OrderBy(x => angles).ToList<float>();
        angles = angles.GetRange(0, 3);
        foreach (float angle in angles) {
            print("new angle: " + angle);
        }
    }


    public static Vector3 GetNormalizedDirection(Vector3 start, Vector3 end) {
        return (end - start).normalized;
    }
}
