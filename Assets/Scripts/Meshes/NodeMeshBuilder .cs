using UnityEngine;
using Nodes.MeshHandler.Data;
using Paths;
using Paths.MeshHandler;
using Path.Entities;

namespace Nodes.MeshHandler {

    public class NodeMeshBuilder {

        public static Mesh CreateNodeMesh(NodeObject node)
        {
            NodeMeshData nodeMeshData = new(node);
            CombineInstance[] meshes = new CombineInstance[node.ConnectedPaths.Count];

            int i = 0;
            foreach (PathObject connectedPath in node.ConnectedPaths)
            {
                MeshData meshData = new();
                nodeMeshData.PopulateMesh(meshData, connectedPath);

                MeshUtilities.PopulateMeshTriangles(meshData);
                MeshUtilities.PopulateMeshUvs(meshData);
                meshes[i].mesh = MeshUtilities.LoadMesh(meshData);
                i++;
            }

            Mesh mesh = new();
            mesh.CombineMeshes(meshes, true, false);
            return mesh;
        }
    }
}
 
