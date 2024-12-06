using UnityEngine;
using Spline.Utils;

namespace Spline.Entities
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class Segment1 : MonoBehaviour
    {

        [SerializeField]
        Transform controlNode;
        [SerializeField]
        float ratio;

        [SerializeField]
        Node startNode;
        [SerializeField]
        Transform startControlNode;
        [SerializeField]
        Node endNode;
        [SerializeField]
        Transform endControlNode;

        [SerializeField]
        Mesh2D shape2D;

        [SerializeField]
        int resolution;

        Mesh mesh;
        OrientedPoint[] path;

        private void Awake()
        {
            mesh = new Mesh();
            mesh.name = "Segment";
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        public void Init(Node startNode, Node endNode, Transform controlNode)
        {
            this.startNode = startNode;
            this.controlNode = controlNode;
            this.endNode = endNode;
        }


        private void Update()
        {
            path = new OrientedPoint[resolution];

            // controlNode.position = (startNode.position + endNode.position) / 2;


            Vector3 startControlNodeOrientation = (controlNode.position - startNode.position);
            Vector3 endControlNodeOrientation = (controlNode.position - endNode.position);

            Debug.DrawLine(controlNode.position, startNode.position);
            Debug.DrawLine(controlNode.position, endNode.position);



            Vector3 startControlNodePosition = (ratio * startControlNodeOrientation.normalized * startControlNodeOrientation.magnitude / 2) + startNode.position;
            Vector3 endControlNodePosition = (ratio * endControlNodeOrientation.normalized * endControlNodeOrientation.magnitude / 2) + endNode.position;

            startControlNode.position = startControlNodePosition;
            endControlNode.position = endControlNodePosition;

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                path[i] = Bezier.GetOP(startNode.transform, startControlNodePosition, endNode.transform, endControlNodePosition, t);
            }
            print(Bezier.GetAproxLength(path));
            Extrude(mesh, shape2D, path);
        }

        public void Extrude(Mesh mesh, Mesh2D shape, OrientedPoint[] path)
        {
            int vertsInShape = shape.VertexCount;
            int edgeLoops = path.Length;
            int segments = edgeLoops - 1;
            int vertCount = vertsInShape * edgeLoops;
            int triCount = shape.LineCount * segments;
            int triIndexCount = triCount * 3;
            float uSpam = shape.CalculateUspan();

            int[] triangleIndices = new int[triIndexCount];
            Vector3[] vertices = new Vector3[vertCount];
            Vector3[] normals = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];

            /* Generation code goes here */

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangleIndices;
            mesh.normals = normals;
            mesh.uv = uvs;

            // Vertices
            for (int i = 0; i < edgeLoops; i++)
            {
                int offset = i * vertsInShape;
                for (int j = 0; j < vertsInShape; j++)
                {
                    int id = offset + j;
                    float t = i / (float)segments;

                    vertices[id] = path[i].LocalToWorldPosition(shape.vertices[j].point);
                    normals[id] = path[i].LocalToWorldDirection(shape.vertices[j].normal);
                    uvs[id] = new Vector2(shape.vertices[j].uCoord, t * Bezier.GetAproxLength(path) / uSpam);
                }
            }

            // Triangles
            int ti = 0;
            for (int i = 0; i < segments; i++)
            {
                int offset = i * vertsInShape;
                for (int l = 0; l < shape.LineCount; l += 2)
                {
                    int a = offset + shape.lines[l] + vertsInShape;
                    int b = offset + shape.lines[l];
                    int c = offset + shape.lines[l + 1];
                    int d = offset + shape.lines[l + 1] + vertsInShape;

                    triangleIndices[ti] = a; ti++;
                    triangleIndices[ti] = d; ti++;
                    triangleIndices[ti] = c; ti++;
                    triangleIndices[ti] = c; ti++;
                    triangleIndices[ti] = b; ti++;
                    triangleIndices[ti] = a; ti++;
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangleIndices, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
        }
    }
}