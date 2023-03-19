using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rafael.Utils;
using System.Linq;

public class TestVectorScript : MonoBehaviour
{
    GameObject center;
    GameObject endMain;
    GameObject endLeft;
    GameObject endMid;
    GameObject endRight;

    Vector3 centerPos;
    Vector3 endMainPos;
    Vector3 endLeftPos;
    Vector3 endMidPos;
    Vector3 endRightPos;

    // Start is called before the first frame update
    void Start()
    {
        center = RafaelUtils.CreateSphere(Vector3.zero, "center", transform, 1);
        endMain = RafaelUtils.CreateSphere(Vector3.zero, "end main", transform, 1);
        endLeft = RafaelUtils.CreateSphere(Vector3.zero, "end left", transform, 1);
        endMid = RafaelUtils.CreateSphere(Vector3.zero, "end mid", transform, 1);
        endRight = RafaelUtils.CreateSphere(Vector3.zero, "end right", transform, 1);

        center.transform.position = Vector3.zero;
        endMain.transform.position = new Vector3(0, 0, -10);
        endLeft.transform.position = new Vector3(-10, 0, 10);
        endMid.transform.position = new Vector3(0, 0, 10);
        endRight.transform.position = new Vector3(10, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Dictionary<float, string> angles = new Dictionary<float, string>();

        centerPos = center.transform.position;
        endMainPos = endMain.transform.position;
        endLeftPos = endLeft.transform.position;
        endMidPos = endMid.transform.position;
        endRightPos = endRight.transform.position;

        Vector3 mainRoadDir = GetNormalizedDirection(centerPos, endMainPos);
        Vector3 leftRoadDir = GetNormalizedDirection(centerPos, endLeftPos);
        Vector3 midRoadDir = GetNormalizedDirection(centerPos, endMidPos);
        Vector3 rightRoadDir = GetNormalizedDirection(centerPos, endRightPos);

        float leftAngle = Vector3.SignedAngle(mainRoadDir, leftRoadDir, Vector3.up);
        float midAngle = Vector3.SignedAngle(mainRoadDir, midRoadDir, Vector3.up);
        float rightAngle = Vector3.SignedAngle(mainRoadDir, rightRoadDir, Vector3.up);

        if (leftAngle < 0) leftAngle += 360;
        if (midAngle < 0) midAngle += 360;
        if (rightAngle < 0) rightAngle += 360;

        angles.Add(rightAngle, "rightAngle");
        angles.Add(midAngle, "midAngle");
        angles.Add(leftAngle, "leftAngle");

        Debug.DrawLine(centerPos, endMainPos, Color.blue);
        Debug.DrawLine(centerPos, endLeftPos, Color.cyan);
        Debug.DrawLine(centerPos, endMidPos, Color.green);
        Debug.DrawLine(centerPos, endRightPos, Color.black);

        PrintAngles(angles);

    }

    void PrintAngles(Dictionary<float, string> angles) {

        angles = angles.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        //print("###################");
        //print(angles.First().Value + " " + angles.First().Key);
        //print(angles.Last().Value + " " + angles.Last().Key);

        Dictionary<float, string> adjacentAngles = new();
        adjacentAngles.Add(angles.First().Key, angles.First().Value);
        adjacentAngles.Add(angles.Last().Key, angles.Last().Value);

        print("###################");
        print("len: " + adjacentAngles.Count());
        print(adjacentAngles.First().Value + " " + adjacentAngles.First().Key);
        print(adjacentAngles.Last().Value + " " + adjacentAngles.Last().Key);
    }


    public static Vector3 GetNormalizedDirection(Vector3 start, Vector3 end) {
        return (end - start).normalized;
    }
}
