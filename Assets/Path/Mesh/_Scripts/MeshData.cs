using System.Collections.Generic;
using UnityEngine;

public class MeshData 
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();

    public void AddVertice(Vector3 vertice) {
        this.vertices.Add(vertice);
    }

    public void AddTriangles(int[] triangles) {
        this.triangles.AddRange(triangles);
    }

    public void AddUvs(Vector2[] uvs) {
        this.uvs.AddRange(uvs);
    }
}
