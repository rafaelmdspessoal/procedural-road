using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{    
    public GameObject node;

    public RoadSegmentObject CreateSegment(Vector3 startPos, Vector3 endPos, RoadSegment roadSegment)
    {
        GameObject node_1 = Instantiate(node, startPos, Quaternion.identity);
        GameObject node_2 = Instantiate(node, endPos, Quaternion.identity);

        return roadSegment.CreateRoadSegment(node_1, node_2);
    }

    public RoadSegmentObject CreateSegment(GameObject existingNode, Vector3 position, RoadSegment roadSegment)
    {
        GameObject node_1 = Instantiate(node, position, Quaternion.identity);
        GameObject node_2 = existingNode;

        return roadSegment.CreateRoadSegment(node_1, node_2);
    }

    public RoadSegmentObject CreateSegment(Vector3 position, GameObject existingNode, RoadSegment roadSegment)
    {
        GameObject node_1 = existingNode;
        GameObject node_2 = Instantiate(node, position, Quaternion.identity); 

        return roadSegment.CreateRoadSegment(node_1, node_2);
    }

    public RoadSegmentObject CreateSegment(GameObject node_1, GameObject node_2, RoadSegment roadSegment)
    {
        return roadSegment.CreateRoadSegment(node_1, node_2);
    }

    public Node SplitSegment(RoadSegmentObject roadSegment, Vector3 positionToSplit)
    {
        GameObject newNodeObj = Instantiate(node, positionToSplit, Quaternion.identity);    
        RoadSegment roadSegmentSO = roadSegment.RoadSegmentSO;

        GameObject startNode = roadSegment.StartNode.transform.gameObject;
        GameObject endNode = roadSegment.EndNode.transform.gameObject;

        //roadSegment.StartNode.RemoveRoadSegment(roadSegment);
        //roadSegment.EndNode.RemoveRoadSegment(roadSegment);

        CreateSegment(startNode, newNodeObj, roadSegmentSO);
        CreateSegment(endNode, newNodeObj, roadSegmentSO);
        
        RemoveSegment(roadSegment);
        Node newNode = newNodeObj.GetComponent<Node>();

        return newNode;
    }

    public void RemoveSegment(RoadSegmentObject roadSegment)
    {
        Destroy(roadSegment.gameObject);
    }
}
