using System.Collections.Generic;
using UnityEngine;

namespace Spline.Entities
{
    public class Node : MonoBehaviour
    {
        [SerializeField] private List<Segment> connectedSegmentsList = new();

        public bool HasConnection => connectedSegmentsList.Count > 0;
        public bool HasIntersection => connectedSegmentsList.Count > 1;
        public Vector3 position => transform.position;
        public Vector3 direction
        {
            get { return transform.forward; }
        }
        public Quaternion orientation
        {
            set { transform.rotation = value; }
        }
        public void AddSegment(Segment segment)
        {
            if (!connectedSegmentsList.Contains(segment))
            {
                connectedSegmentsList.Add(segment);
            }
        }
        //public Dictionary<float, PathObject> GetAdjacentPathsTo(PathObject pathObject)
        //{
        //    Dictionary<float, PathObject> connectedPathsDict = new();

        //    if (!HasIntersection) return connectedPathsDict;

        //    if (pathObject != null)
        //    {
        //        Vector3 pathObjectDirection = Position - pathObject.ControlPosition;

        //        foreach (PathObject connectedPath in connectedPathList)
        //        {
        //            if (connectedPath != pathObject)
        //            {
        //                Vector3 connectedPathDirection = Position - connectedPath.ControlPosition;
        //                float angle = Vector3.SignedAngle(pathObjectDirection, connectedPathDirection, transform.up);
        //                if (angle < 0) angle += 360;
        //                if (!connectedPathsDict.ContainsKey(angle))
        //                    connectedPathsDict.Add(angle, connectedPath);
        //            }
        //        }
        //    }

        //    connectedPathsDict = connectedPathsDict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

        //    Dictionary<float, PathObject> adjacentPaths = new()
        //    {
        //        { connectedPathsDict.First().Key, connectedPathsDict.First().Value }
        //    };

        //    if (connectedPathsDict.Count > 1)
        //        adjacentPaths.Add(connectedPathsDict.Last().Key, connectedPathsDict.Last().Value);

        //    return adjacentPaths;
        //}
       
    }
}