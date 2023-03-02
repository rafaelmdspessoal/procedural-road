using System.Collections;
using UnityEngine;

public static class RoadUtilities {

    public static Vector3 GetRoadLeftSideVertice(float roadWidth, Vector3 centerVertice, Vector3 startVertice) {
        Vector3 verticeDirection = GetVerticeNormalizedDirection(startVertice, centerVertice);
        Vector3 left = new Vector3(-verticeDirection.z, verticeDirection.y, verticeDirection.x);
        Vector3 leftSideVertice = centerVertice + .5f * roadWidth * left;
        return leftSideVertice;
    }

    public static Vector3 GetRoadRightSideVertice(float roadWidth, Vector3 centerVertice, Vector3 startVertice) {
        Vector3 verticeDirection = GetVerticeNormalizedDirection(startVertice, centerVertice);
        Vector3 right = new Vector3(-verticeDirection.z, verticeDirection.y, verticeDirection.x) * -1f;
        Vector3 rightSideVertice = centerVertice + .5f * roadWidth * right;
        return rightSideVertice;
    }

    public  static Vector3 GetVerticeNormalizedDirection(Vector3 controlPosition, Vector3 verticePosition) {
        return (verticePosition - controlPosition).normalized;
    }

    private static float GetControlNodeAngle(Vector3 startVertice, Vector3 endVertice, Vector3 controlVertice) {
        Vector3 startVerticeDirection = GetVerticeNormalizedDirection(controlVertice, startVertice);
        Vector3 endVerticeDirection = GetVerticeNormalizedDirection(controlVertice, endVertice);

        float angle = Vector3.Angle(startVerticeDirection, endVerticeDirection);
        return angle * Mathf.Deg2Rad;
    }

}