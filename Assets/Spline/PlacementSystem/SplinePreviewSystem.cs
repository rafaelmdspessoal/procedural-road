using Paths.MeshHandler;
using Paths.Preview.MeshHandler;
using System;
using System.IO;
using UnityEngine;


namespace Spline.Placement
{
    [RequireComponent(typeof(LineRenderer))]
    public class SplinePreviewSystem : MonoBehaviour
    {
        public static SplinePreviewSystem Instance { get; private set; }
        private LineRenderer lineRenderer;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();            
        }

        public void DisplayTemporarySpline(Vector3[] path)
        {
            lineRenderer.positionCount = path.Length;
            for (int i = 0; i < path.Length; i++)
            {
                lineRenderer.SetPosition(i, path[i]);
            }
        }

        public void StopPreview()
        {
            lineRenderer.positionCount = 0;
        }
    }
}