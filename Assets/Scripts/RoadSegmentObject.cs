using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RoadSegmentObject : MonoBehaviour
{
    [SerializeField]
    private Node startNode;
    [SerializeField]
    private Node endNode;
    private GameObject controlNode;
    private RoadSegment roadSegmentSO;
    private Material roadMaterial;

    public Vector3 startMidRoadMesh;
    public Vector3 startLeftRoadMesh;
    public Vector3 startRightRoadMesh;
    
    public Vector3 startMidNodeMesh;
    public Vector3 startLeftNodeMesh;
    public Vector3 startRightNodeMesh;

    public Vector3 endMidRoadMesh;
    public Vector3 endLeftRoadMesh;
    public Vector3 endRightRoadMesh;

    public Vector3 endMidNodeMesh;
    public Vector3 endLeftNodeMesh;
    public Vector3 endRightNodeMesh;

    public GameObject[] meshEdgePoints = new GameObject[12];


    public Node StartNode
    {
        get
        {
            return startNode;
        }
    }

    public Node EndNode
    {
        get
        {
            return endNode;
        }
    }
    public GameObject ControlNode
    {
        get
        {
            return controlNode;
        }
    }

    float GetMinAngleFor(Node node, List<RoadObject> adjacentSegments)
    {
        Vector3 segmentDir = Direction(node);
        float currentAngle = 360;
        foreach (RoadObject adjacentSegment in adjacentSegments)
        {
            //float angle = Vector3.Angle(segmentDir, adjacentSegment.Direction(node));
            //if (angle < currentAngle)
            //{
            //    currentAngle = angle;
            //}
        }
        return currentAngle;
    }

    float GetMaxRoadWidthIn(List<RoadObject> adjacentSegments)
    {
        float maxWidth = 0;
        foreach (RoadObject segmentObject in adjacentSegments)
        {
            //float segmentWidth = segmentObject.RoadSegmentSO.roadWidth;
            //if (segmentWidth > maxWidth) maxWidth = segmentWidth;
        }
        return maxWidth;
    }

    public Vector3 Direction(Node node)
    {
        Vector3 dir = node.Position - controlNode.transform.position;
        dir.Normalize();
        return dir;
    }

    public void SetRoadMeshEdgePoints(Node node)
    {        
        float roadWidth = roadSegmentSO.roadWidth;
        //List<RoadObject> adjacentRoads = node.GetAdjacentRoadsTo(this);

        //Vector3 segmentDir = Direction(node);

        //float minAngale = GetMinAngleFor(node, adjacentRoads);
        //float maxWidth = GetMaxRoadWidthIn(adjacentRoads);
        //float offsetDistance = maxWidth / Mathf.Sin(Mathf.Deg2Rad * minAngale);
        //if(minAngale >= 90)
        //{
        //    offsetDistance = maxWidth / 2;
        //}

        if (node == startNode)
        {
            //startMidRoadMesh = node.GetNodeSizeForRoad(this, offsetDistance);

            //Vector3 left = new Vector3(-segmentDir.z, segmentDir.y, segmentDir.x);

            //startLeftRoadMesh = startMidRoadMesh + .5f * roadWidth * left;
            //startRightRoadMesh = startMidRoadMesh - .5f * roadWidth * left;

            //startMidNodeMesh = startMidRoadMesh + .5f * roadWidth * segmentDir;
            //startLeftNodeMesh = startMidNodeMesh + .5f * roadWidth * left;
            //startRightNodeMesh = startMidNodeMesh - .5f * roadWidth * left;
        }
        else
        {
            //endMidRoadMesh = node.GetNodeSizeForRoad(this, offsetDistance);

            //Vector3 left = new Vector3(-segmentDir.z, segmentDir.y, segmentDir.x);

            //endLeftRoadMesh = endMidRoadMesh + .5f * roadWidth * left;
            //endRightRoadMesh = endMidRoadMesh - .5f * roadWidth * left;

            //endMidNodeMesh = endMidRoadMesh + .5f * roadWidth * segmentDir;
            //endLeftNodeMesh = endMidNodeMesh + .5f * roadWidth * left;
            //endRightNodeMesh = endMidNodeMesh - .5f * roadWidth * left;
        }
    }

    public bool IsStartNode(Node node)
    {
        return node == StartNode;
    }

    public void Init(
        GameObject startNode, 
        GameObject endNode,
        Vector3 controlNodePos, 
        RoadSegment roadSegmentSO, 
        Material roadMaterial
    )
    {
        //startNode.transform.name = "Start Node";
        //endNode.transform.name = "End Node";

        //this.startNode = startNode.GetComponent<Node>();
        //this.endNode = endNode.GetComponent<Node>();
        //this.roadSegmentSO = roadSegmentSO;
        //this.roadMaterial = roadMaterial;

        //controlNode = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //controlNode.transform.position = controlNodePos;
        //controlNode.transform.localScale = (roadSegmentSO.roadWidth / 2) * Vector3.one;
        //controlNode.transform.parent = transform;

        //this.startNode.AddRoadSegment(this);
        //this.endNode.AddRoadSegment(this);
        //UpdateRoad();
    }

    public float SegmentWidth
    {
        get { return roadSegmentSO.roadWidth; }
    }

    public RoadSegment RoadSegmentSO
    {
        get { return roadSegmentSO; }
        set { roadSegmentSO = value; }
    }

    public Material RoadMaterial
    {
        get { return roadMaterial; }
        set {
            Material newMat = new Material(value);
            roadMaterial = newMat;
        }
    }

    public Vector3 ControlPointPosition()
    {
        return controlNode.transform.position;
    }

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    void UpdateMeshEdgePoints()
    {
        //meshEdgePoints[0].transform.position = startMidRoadMesh;
        //meshEdgePoints[1].transform.position = startLeftRoadMesh;
        //meshEdgePoints[2].transform.position = startRightRoadMesh;

        //meshEdgePoints[3].transform.position = startMidNodeMesh;
        //meshEdgePoints[4].transform.position = startLeftNodeMesh;
        //meshEdgePoints[5].transform.position = startRightNodeMesh;

        //meshEdgePoints[6].transform.position = endMidRoadMesh;
        //meshEdgePoints[7].transform.position = endLeftRoadMesh;
        //meshEdgePoints[8].transform.position = endRightRoadMesh;

        //meshEdgePoints[9].transform.position = endMidNodeMesh;
        //meshEdgePoints[10].transform.position = endLeftNodeMesh;
        //meshEdgePoints[11].transform.position = endRightNodeMesh;


        //meshEdgePoints[0].transform.name = "startMidRoadMesh";
        //meshEdgePoints[1].transform.name = "startLeftRoadMesh";
        //meshEdgePoints[2].transform.name = "startRightRoadMesh";

        //meshEdgePoints[3].transform.name = "startMidNodeMesh";
        //meshEdgePoints[4].transform.name = "startLeftNodeMesh";
        //meshEdgePoints[5].transform.name = "startRightNodeMesh";

        //meshEdgePoints[6].transform.name = "endMidRoadMesh";
        //meshEdgePoints[7].transform.name = "endLeftRoadMesh";
        //meshEdgePoints[8].transform.name = "endRightRoadMesh";

        //meshEdgePoints[9].transform.name = "endMidNodeMesh";
        //meshEdgePoints[10].transform.name = "endLeftNodeMesh";
        //meshEdgePoints[11].transform.name = "endRightNodeMesh";


        //meshEdgePoints[0].transform.parent = startNode.transform;
        //meshEdgePoints[1].transform.parent = startNode.transform;
        //meshEdgePoints[2].transform.parent = startNode.transform;

        //meshEdgePoints[3].transform.parent = startNode.transform;
        //meshEdgePoints[4].transform.parent = startNode.transform;
        //meshEdgePoints[5].transform.parent = startNode.transform;

        //meshEdgePoints[6].transform.parent = endNode.transform;
        //meshEdgePoints[7].transform.parent = endNode.transform;
        //meshEdgePoints[8].transform.parent = endNode.transform;

        //meshEdgePoints[9].transform.parent = endNode.transform;
        //meshEdgePoints[10].transform.parent = endNode.transform;
        //meshEdgePoints[11].transform.parent = endNode.transform;
    }

    private void Start()
    {
        //UpdateRoad();
        //for (int i = 0; i < 12; i++)
        //{
        //    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    obj.transform.localScale = Vector3.one * 0.3f;
        //    obj.transform.parent = transform;
        //    meshEdgePoints[i] = obj;
        //}
        //UpdateMeshEdgePoints();
        //transform.position = new Vector3(
        //    transform.position.x,
        //    transform.position.y + 0.01f,
        //    transform.position.z
        //);
    }

    private void Update()
    {
        UpdateRoad();
        UpdateMeshEdgePoints();
        controlNode.transform.position = (startNode.Position + endNode.Position) / 2;
    }


    public void UpdateRoad()
    {
        Mesh mesh = roadSegmentSO.CreateRoadMesh(this);
        int textureRepead = Mathf.RoundToInt(roadSegmentSO.tiling * Bezier.GetLengh(
            startNode.Position,
            EndNode.Position
        ) * roadSegmentSO.spacing * .005f);
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = roadMaterial;
        meshRenderer.material.mainTextureScale = new Vector2(.5f, textureRepead);
        meshRenderer.material.mainTextureOffset = new Vector2(0, 0);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
