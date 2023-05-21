using Path.Entities;
using System.Collections;
using UnityEngine;

namespace Path
{
    public interface IPath 
    {
        public void PlacePath(NodeObject startNode, NodeObject endNode, Vector3 controlPosition);
        public void BuildPath();
        public void UpdatePath();
        public void RemovePath();
    }
}