using UnityEngine;
using Nodes.MeshHandler.Data;
using Roads;
using Roads.MeshHandler;

namespace Nodes.MeshHandler {

    public class NodeMeshBuilder {

        public static Mesh CreateNodeMesh(Node node)
        {
            NodeMeshData nodeMeshData = new(node);
            CombineInstance[] meshes = new CombineInstance[node.ConnectedRoads.Count];

            int i = 0;
            foreach (RoadObject connectedRoad in node.ConnectedRoads)
            {
                MeshData meshData = new();
                nodeMeshData.PopulateMesh(meshData, connectedRoad);

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
 
