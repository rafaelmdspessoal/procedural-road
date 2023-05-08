using UnityEngine;
using UnityEditor;
using Nodes;

[CustomEditor(typeof(AIDirector))]
public class AIDirectorEditor : Editor
{
    Node startNode;
    Node endNode;

    public override void OnInspectorGUI()
    {
        AIDirector aIDirector = (AIDirector)target;

        if(GUILayout.Button("Find Path"))
        {
            startNode = aIDirector.GetRandomNode();
            endNode = startNode;
            if(startNode == null)
            {
                Debug.Log("StartNode Missing!");
                return;
            }
            while (endNode.Equals(startNode))
            {
                endNode = aIDirector.GetRandomNode();
            }

            aIDirector.GetPathBetween(startNode, endNode);
            Debug.Log("Fount a Path!");
        }
    }    
}