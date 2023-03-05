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

    Vector3 centerPos;
    Vector3 endMainPos;
    Vector3 endLeftPos;
    Vector3 endMidPos;
    Vector3 endRightPos;

    // Start is called before the first frame update
    void Start()
    {
        center = RafaelUtils.CreateSphere(Vector3.zero, "center", 1);
        endMain = RafaelUtils.CreateSphere(Vector3.zero, "end main", 1);
        endLeft = RafaelUtils.CreateSphere(Vector3.zero, "end left", 1);
        endMid = RafaelUtils.CreateSphere(Vector3.zero, "end mid", 1);
        endRight = RafaelUtils.CreateSphere(Vector3.zero, "end right", 1);

        center.transform.position = Vector3.zero;
        endMain.transform.position = new Vector3(0, 0, -1);
        endLeft.transform.position = new Vector3(-1, 0, 1);
        endMid.transform.position = new Vector3(0, 0, 1);
        endRight.transform.position = new Vector3(1, 0, 0);
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

        var ordered = angles.OrderBy(x => Mathf.Abs(x.Key)).ToDictionary(x => x.Key, x => x.Value).Take(2);
        //print("###################");
        //print(angles.First().Value + " " + angles.First().Key);
        //print(angles.Last().Value + " " + angles.Last().Key);

        print("###################");
        print("len: " + ordered.Count());
        print(ordered.First().Value + " " + ordered.First().Key);
        print(ordered.Last().Value + " " + ordered.Last().Key);
    }


    public static Vector3 GetNormalizedDirection(Vector3 start, Vector3 end) {
        return (end - start).normalized;
    }
}
