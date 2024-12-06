using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class Mesh2D : ScriptableObject
{
    [System.Serializable]
    public class Vertex
    {
        public Vector2 point;
        public Vector2 normal;
        public float uCoord;
    }

    public Vertex[] vertices;
    public int[] lines;

    public int VertexCount => vertices.Length;
    public int LineCount => lines.Length;

    public float CalculateUspan()
    {
        float dist = 0f;
        for (int i = 0; i < LineCount; i+=2)
        {
            Vector2 a = vertices[lines[i]].point;
            Vector2 b = vertices[lines[i + 1]].point;

            dist += (a - b).magnitude;
        }
        return dist;
    }
}
