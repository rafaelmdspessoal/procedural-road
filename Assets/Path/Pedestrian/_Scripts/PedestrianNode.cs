using Path.AI;
using Path.Entities.Meshes;
using Path.Entities.Pedestrian.SO;
using System.Collections.Generic;
using UnityEngine;

namespace Path.Entities.Pedestrian
{
    public class PedestrianNode : NodeObject
    {
        private void Start()
        {
            pathFor = PathFor.Pedestrian;
        }

        protected override void CreatePathNodeFor(PathObject pathObject)
        {
            PedestrianPath pedestrianPath = pathObject as PedestrianPath;
            PedestrianPathSO vehiclePathSO = pathObject.PathSO as PedestrianPathSO;

            PedestrianPathNode startPathNode = Instantiate(
                vehiclePathSO.pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PedestrianPathNode>();

            PedestrianPathNode endPathNode = Instantiate(
                vehiclePathSO.pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PedestrianPathNode>();

            if (IsStartNodeOf(pedestrianPath))
            {
                startPathNode.Init(PathNodeObject.OnPathPosition.StartNodeStartPath);
                endPathNode.Init(PathNodeObject.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                startPathNode.Init(PathNodeObject.OnPathPosition.EndNodeStartPath);
                endPathNode.Init(PathNodeObject.OnPathPosition.EndNodeEndPath);
            }
            pedestrianPathNodesDict.Add(pedestrianPath, new List<PedestrianPathNode> { startPathNode, endPathNode });
        }
        protected override void UpdatePathPostions(PathObject pathObject)
        {
            MeshEdje center;
            MeshEdje left;

            PathNodeObject startPathNode;
            PathNodeObject endPathNode;

            int laneWidth = pathObject.PathSO.laneWidth;

            if (IsStartNodeOf(pathObject))
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartCenter);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartLeft);

                startPathNode = GetPedestrianPathNodeFor(pathObject, PathNodeObject.OnPathPosition.StartNodeStartPath);
                endPathNode = GetPedestrianPathNodeFor(pathObject, PathNodeObject.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndCenter);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndLeft);

                startPathNode = GetPedestrianPathNodeFor(pathObject, PathNodeObject.OnPathPosition.EndNodeStartPath);
                endPathNode = GetPedestrianPathNodeFor(pathObject, PathNodeObject.OnPathPosition.EndNodeEndPath);
            }

            Vector3 centerPos = center.Position;
            Vector3 leftPos = left.Position;

            Vector3 leftDir = (leftPos - centerPos).normalized;

            Vector3 startPathPosition = centerPos + leftDir * laneWidth / 2;
            Vector3 endPathPosition = centerPos - leftDir * laneWidth / 2;

            startPathNode.transform.position = startPathPosition;
            endPathNode.transform.position = endPathPosition;

            startPathNode.transform.rotation = Quaternion.LookRotation(center.Direction);
            endPathNode.transform.rotation = Quaternion.LookRotation(center.Direction);
        }
        public override void ConnectPathNodes()
        {
            PedestrianPathNode thisPathStart;
            PedestrianPathNode thisPathEnd;

            PedestrianPathNode nextPathStart;

            foreach (PathObject connectedPath in ConnectedPaths)
            {
                if (IsStartNodeOf(connectedPath))
                {
                    thisPathStart = GetPedestrianPathNodeFor(connectedPath, PathNodeObject.OnPathPosition.StartNodeStartPath);
                    thisPathEnd = GetPedestrianPathNodeFor(connectedPath, PathNodeObject.OnPathPosition.StartNodeEndPath);
                }
                else
                {
                    thisPathStart = GetPedestrianPathNodeFor(connectedPath, PathNodeObject.OnPathPosition.EndNodeStartPath);
                    thisPathEnd = GetPedestrianPathNodeFor(connectedPath, PathNodeObject.OnPathPosition.EndNodeEndPath);
                }

                if (ConnectedPaths.Count == 1)
                {
                    thisPathEnd.AddPathNode(thisPathStart);
                    return;
                }

                foreach (PathObject nextConnectedPath in ConnectedPaths)
                {
                    if (connectedPath == nextConnectedPath) continue;
                    if (IsStartNodeOf(nextConnectedPath))
                        nextPathStart = GetPedestrianPathNodeFor(nextConnectedPath, PathNodeObject.OnPathPosition.StartNodeStartPath);
                    else
                        nextPathStart = GetPedestrianPathNodeFor(nextConnectedPath, PathNodeObject.OnPathPosition.EndNodeStartPath);

                    thisPathEnd.AddPathNode(nextPathStart);
                }
            }
        }
        public void ConnectToSidewalk(PedestrianPathNode sidewalkNode)
        {
            foreach (PedestrianPathNode pedestrianPathNode in GetAllPedestrianPathNodes())
            {
                pedestrianPathNode.AddPathNode(sidewalkNode);
                sidewalkNode.AddPathNode(pedestrianPathNode);
            }
        }
    }
}
