using UnityEngine;
using UnityEditor;
using Path.Entities;

namespace Path.AI
{
    [CustomEditor(typeof(AIDirector))]
    public class AIDirectorEditor : Editor
    {
        NodeObject startNode;
        NodeObject endNode;

        public override void OnInspectorGUI()
        {
            AIDirector aIDirector = (AIDirector)target;
            DrawDefaultInspector();
            if (GUILayout.Button("Find Path"))
            {
                startNode = aIDirector.GetRandomNode();
                endNode = startNode;
                if (startNode == null)
                {
                    Debug.Log("StartNode Missing!");
                    return;
                }
                while (endNode.Equals(startNode))
                {
                    endNode = aIDirector.GetRandomNode();
                }

                aIDirector.GetPathBetween(startNode, endNode);
            }
        }
    }
}
