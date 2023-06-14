using Path.Entities.Meshes;
using Path.Entities.Pedestrian;
using Path.Entities.Pedestrian.SO;
using Path.Entities.Vehicle.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

namespace Path.Entities.Vehicle
{
    public class VehicleNode : NodeObject
    {
        public bool hasPathWithSidewalk = false;
        private void Start()
        {
            pathFor = PathFor.Vehicle;
        }
        protected override void CreatePathNodeFor(PathObject pathObject)
        {
            VehiclePath vechiclePath= pathObject as VehiclePath;
            VehiclePathSO vehiclePathSO = pathObject.PathSO as VehiclePathSO;

            VehiclePathNode startVechiclePathNode = Instantiate(
                vehiclePathSO.pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<VehiclePathNode>();

            VehiclePathNode endVehiclePathNode = Instantiate(
                vehiclePathSO.pathNodePrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<VehiclePathNode>();

            if (IsStartNodeOf(vechiclePath))
            {
                startVechiclePathNode.Init(PathNodeObject.OnPathPosition.StartNodeStartPath);
                endVehiclePathNode.Init(PathNodeObject.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                startVechiclePathNode.Init(PathNodeObject.OnPathPosition.EndNodeStartPath);
                endVehiclePathNode.Init(PathNodeObject.OnPathPosition.EndNodeEndPath);
            }
            vehiclePathNodesDict.Add(vechiclePath, new List<VehiclePathNode> { startVechiclePathNode, endVehiclePathNode });

            // Handle sidewalks 
            if (!vehiclePathSO.hasSidewalk) return;
            hasPathWithSidewalk = true;
            PedestrianPathNode startPedestrianPathNode = Instantiate(
                vehiclePathSO.pedestrianPathNode,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PedestrianPathNode>();

            PedestrianPathNode endPedestrianPathNode = Instantiate(
                vehiclePathSO.pedestrianPathNode,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<PedestrianPathNode>();

            if (IsStartNodeOf(vechiclePath))
            {
                startPedestrianPathNode.Init(PathNodeObject.OnPathPosition.StartNodeStartPath);
                endPedestrianPathNode.Init(PathNodeObject.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                startPedestrianPathNode.Init(PathNodeObject.OnPathPosition.EndNodeStartPath);
                endPedestrianPathNode.Init(PathNodeObject.OnPathPosition.EndNodeEndPath);
            }
            pedestrianPathNodesDict.Add(vechiclePath, new List<PedestrianPathNode> { startPedestrianPathNode, endPedestrianPathNode });

        }
        protected override void UpdatePathPostions(PathObject pathObject)
        {
            base.UpdatePathPostions(pathObject);

            VehiclePath path = pathObject as VehiclePath;
            VehiclePathSO vehiclePathSO = path.PathSO as VehiclePathSO;
            if (!vehiclePathSO.hasSidewalk) return;
            hasPathWithSidewalk = true;
            MeshEdje center;
            MeshEdje left;

            PedestrianPathNode startPathNode;
            PedestrianPathNode endPathNode;

            int width = vehiclePathSO.Width;
            int sidewalkWidth = vehiclePathSO.sidewalkWidth;

            if (IsStartNodeOf(path))
            {
                center = GetMeshEdjeFor(path, MeshEdje.EdjePosition.StartCenter);
                left = GetMeshEdjeFor(path, MeshEdje.EdjePosition.StartLeft);

                startPathNode = GetPedestrianPathNodeFor(path, PathNodeObject.OnPathPosition.StartNodeStartPath);
                endPathNode = GetPedestrianPathNodeFor(path, PathNodeObject.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                center = GetMeshEdjeFor(path, MeshEdje.EdjePosition.EndCenter);
                left = GetMeshEdjeFor(path, MeshEdje.EdjePosition.EndLeft);

                startPathNode = GetPedestrianPathNodeFor(path, PathNodeObject.OnPathPosition.EndNodeStartPath);
                endPathNode = GetPedestrianPathNodeFor(path, PathNodeObject.OnPathPosition.EndNodeEndPath);
            }

            Vector3 centerPos = center.Position;
            Vector3 leftPos = left.Position;

            Vector3 leftDir = (leftPos - centerPos).normalized;

            Vector3 startPathPosition = centerPos + leftDir * (width / 2 - sidewalkWidth + sidewalkWidth / 2f);
            Vector3 endPathPosition = centerPos - leftDir * (width / 2 - sidewalkWidth + sidewalkWidth / 2f);

            startPathNode.transform.position = startPathPosition;
            endPathNode.transform.position = endPathPosition;

            startPathNode.transform.rotation = Quaternion.LookRotation(center.Direction);
            endPathNode.transform.rotation = Quaternion.LookRotation(center.Direction);
        }
        public override void ConnectPathNodes()
        {
            base.ConnectPathNodes();

            PedestrianPathNode thiPathNodeLeft;
            PedestrianPathNode thiPathNodeRight;

            PedestrianPathNode rightPathNodeLeft;
            PedestrianPathNode leftPathNodeRight;
            
            foreach (PathObject path in ConnectedPaths)
            {
                VehiclePath thisPath = path as VehiclePath;
                VehiclePathSO vehiclePathSO = thisPath.PathSO as VehiclePathSO;
                if (!vehiclePathSO.hasSidewalk) return;

                if (IsStartNodeOf(thisPath))
                {
                    thiPathNodeLeft = GetPedestrianPathNodeFor(thisPath, PathNodeObject.OnPathPosition.StartNodeStartPath);
                    thiPathNodeRight = GetPedestrianPathNodeFor(thisPath, PathNodeObject.OnPathPosition.StartNodeEndPath);
                }
                else
                {
                    thiPathNodeLeft = GetPedestrianPathNodeFor(thisPath, PathNodeObject.OnPathPosition.EndNodeStartPath);
                    thiPathNodeRight = GetPedestrianPathNodeFor(thisPath, PathNodeObject.OnPathPosition.EndNodeEndPath);
                }

                if (!HasIntersection)
                {
                    thiPathNodeRight.AddPathNode(thiPathNodeLeft);
                    thiPathNodeLeft.AddPathNode(thiPathNodeRight);
                    return;
                }

                VehiclePath leftPath = GetAdjacentPathsTo(path).Values.ToList()[0] as VehiclePath;
                VehiclePath rightPath;
                if (ConnectedPaths.Count == 2)
                    rightPath = GetAdjacentPathsTo(path).Values.ToList()[0] as VehiclePath;
                else
                {
                    rightPath = GetAdjacentPathsTo(path).Values.ToList()[1] as VehiclePath;

                    thiPathNodeRight.AddPathNode(thiPathNodeLeft);
                    thiPathNodeLeft.AddPathNode(thiPathNodeRight);
                }

                leftPathNodeRight = HandleLeftPath(thiPathNodeLeft, leftPath);
                rightPathNodeLeft = HandleRightPath(thiPathNodeRight, rightPath);

                // Abort if any adjacent road has no sidewalk
                if (rightPathNodeLeft == null || leftPathNodeRight == null) return;

                // If a road with sidewalk has intersection with two consecutive roads without
                // pedestrian will traverse the midle of the intersection.
                rightPathNodeLeft.RemovePathConnection(leftPathNodeRight);
                leftPathNodeRight.RemovePathConnection(rightPathNodeLeft);
            }
        }

        private PedestrianPathNode HandleRightPath(PedestrianPathNode thiPathNodeRight, VehiclePath rightPath)
        {
            PedestrianPathNode rightPathNodeLeft = default;

            bool hasSidewalk = (rightPath.PathSO as VehiclePathSO).hasSidewalk;
            if (!hasSidewalk) return rightPathNodeLeft;

            if (IsStartNodeOf(rightPath))
                rightPathNodeLeft = GetPedestrianPathNodeFor(rightPath, PathNodeObject.OnPathPosition.StartNodeStartPath);
            else
                rightPathNodeLeft = GetPedestrianPathNodeFor(rightPath, PathNodeObject.OnPathPosition.EndNodeStartPath);

            thiPathNodeRight.AddPathNode(rightPathNodeLeft);
            rightPathNodeLeft.AddPathNode(thiPathNodeRight);
            return rightPathNodeLeft;
        }

        private PedestrianPathNode HandleLeftPath(PedestrianPathNode thiPathNodeLeft, VehiclePath leftPath)
        {
            PedestrianPathNode leftPathNodeRight = default;

            bool hasSidewalk = (leftPath.PathSO as VehiclePathSO).hasSidewalk;
            if (!hasSidewalk) return leftPathNodeRight;

            if (IsStartNodeOf(leftPath))
                leftPathNodeRight = GetPedestrianPathNodeFor(leftPath, PathNodeObject.OnPathPosition.StartNodeEndPath);
            else
                leftPathNodeRight = GetPedestrianPathNodeFor(leftPath, PathNodeObject.OnPathPosition.EndNodeEndPath);

            thiPathNodeLeft.AddPathNode(leftPathNodeRight);
            leftPathNodeRight.AddPathNode(thiPathNodeLeft);
            return leftPathNodeRight;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var items in pedestrianPathNodesDict.Values)
            {
               foreach (var item in items)
                {
                    List<PathNodeObject> connectedNodes = item.GetConnectedNodes();
                    foreach (var node in connectedNodes)
                    {

                        Gizmos.DrawLine(item.Position, node.Position);
                    }
                }
            }
        }
    }
}
