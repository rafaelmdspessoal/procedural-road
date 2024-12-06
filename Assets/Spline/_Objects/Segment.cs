using UnityEngine;
using Spline.Utils;
using Rafael.Utils;

namespace Spline.Entities
{
    public class Segment : MonoBehaviour
    {

        [SerializeField]
        private Node startNode, endNode;

        private Transform controlNode;
        private Transform startControlNode;
        private Transform endControlNode;

        [SerializeField]
        private float controlNodeRatio;
        //[SerializeField]
        private int resolution;

        private OrientedPoint[] path;
        private OrientedPoint[] rightPath;
        private OrientedPoint[] leftPath;


        private SplineManager splineManager;
        private LineRenderer lineRenderer;

        public void Awake()
        {
            controlNodeRatio = 0.552f;
            lineRenderer = GetComponent<LineRenderer>();
            resolution = 21;
            // path = new OrientedPoint[resolution];
        }

        public void Init(Node startNode, Node endNode, Vector3 controlPosition, float controlNodeRatio)
        {
            splineManager = SplineManager.Instance;

            this.startNode = startNode;
            this.endNode = endNode;

            controlNode = RafaelUtils.CreateSphere(
                controlPosition,
                "Control Node",
                transform,
                1).transform;

            startControlNode = RafaelUtils.CreateSphere(
                controlPosition,
                "Start Control Node",
                transform,
                1).transform;

            endControlNode = RafaelUtils.CreateSphere(
                controlPosition,
                "End Control Node",
                transform, 
                1).transform;

            this.startNode.AddSegment(this);
            this.endNode.AddSegment(this);
            this.controlNodeRatio = controlNodeRatio;

            splineManager.AddSegment(this);
            splineManager.AddNode(startNode);
            splineManager.AddNode(endNode);

            CreateSpline();
        }


        private void Update()
        {
        }

        private void CreateSpline()
        { 
            controlNodeRatio = Mathf.Clamp(controlNodeRatio, 0.552f, 1);
            Bezier.PositionControlNodes(
                controlNodeRatio,
                startNode.position,
                endNode.position,
                controlNode.position,
                out Vector3 startControlNodePosition,
                out Vector3 endControlNodePosition);

            startControlNode.position = startControlNodePosition;
            endControlNode.position = endControlNodePosition;

            path = Bezier.CalculateSplineOP(
                startNode.transform,
                startControlNodePosition,
                endControlNodePosition,
                endNode.transform,
                resolution);

            startNode.orientation = path[0].orientation;
            endNode.orientation = path[resolution - 1].orientation;
            controlNode.transform.localRotation = path[10].orientation;

            Vector3 rightPathStart = path[0].LocalToWorldPosition(Vector3.right);
            Vector3 rightPathEnd = path[resolution - 1].LocalToWorldPosition(Vector3.right);
            Vector3 rightControlNodePosition = controlNode.transform.position + controlNode.transform.localRotation * Vector3.right;

            Bezier.PositionControlNodes(
                controlNodeRatio,
                rightPathStart,
                rightPathEnd,
                rightControlNodePosition,
                out Vector3 startRightControlNodePosition,
                out Vector3 endRightControlNodePosition);

            rightPath = Bezier.CalculateSplineOP(
                rightPathStart,
                startNode.transform.up,
                startRightControlNodePosition,
                endRightControlNodePosition,
                rightPathEnd,
                endNode.transform.up,
                resolution);


            Vector3 leftPathStart = path[0].LocalToWorldPosition(Vector3.left);
            Vector3 leftPathEnd = path[resolution - 1].LocalToWorldPosition(Vector3.left);
            Vector3 leftControlNodePosition = controlNode.transform.position + controlNode.transform.localRotation * Vector3.left;

            Bezier.PositionControlNodes(
                controlNodeRatio,
                leftPathStart,
                leftPathEnd,
                leftControlNodePosition,
                out Vector3 startLeftControlNodePosition,
                out Vector3 endLeftControlNodePosition);

            leftPath = Bezier.CalculateSplineOP(
                leftPathStart,
                startNode.transform.up,
                startLeftControlNodePosition,
                endLeftControlNodePosition,
                leftPathEnd,
                endNode.transform.up,
                resolution);


            Extrude(rightPath);
            Extrude(leftPath);
            Extrude(path);
        }

        private void Extrude(OrientedPoint[] path)
        {
            lineRenderer.positionCount = path.Length;
            for (int i = 0; i < path.Length; i++)
            {
                lineRenderer.SetPosition(i, path[i].position);
                RafaelUtils.CreateSphere(path[i].position, "index " + i, this.transform);
            }
        }
    } 
}