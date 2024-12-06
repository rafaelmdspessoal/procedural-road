using System;
using System.Collections.Generic;
using UnityEngine;
using Spline.Entities;

namespace Spline {

    public class SplineManager : MonoBehaviour {

        public static SplineManager Instance { get; private set; }

        private readonly Dictionary<Vector3, Node> nodesDict = new();
        private readonly List<Segment> segmentsList = new();

        //private List<SerializeablePath> pathObjects = new();

        private void Awake() {
            Instance = this;
        }
        public void AddNode(Node node) 
        {
            if (!HasNode(node))
                nodesDict.Add(node.position, node);
        }
        public void AddSegment(Segment segment)
        {
            if (!segmentsList.Contains(segment))
            {
                segmentsList.Add(segment);
            }
        }
        public void RemoveNode(Node node)
        {
            if (HasNode(node))
            {
                nodesDict.Remove(node.position);
                Destroy(node.gameObject);
            }
        }
        private bool HasNode(Node node) => nodesDict.ContainsValue(node);
        public bool HasNode(Vector3 position) => nodesDict.ContainsKey(position);
        public Node TryGetNodeAt(Vector3 position)
        {
            if (HasNode(position))
            {
                return nodesDict.GetValueOrDefault(position);
            }
            return null;
        }
    }
}