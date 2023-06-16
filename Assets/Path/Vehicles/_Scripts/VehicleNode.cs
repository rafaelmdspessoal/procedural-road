using Path.AI;
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
            MeshEdje center;
            MeshEdje left;

            VehiclePathNode startVehiclePathNode;
            VehiclePathNode endVehiclePathNode;

            int laneWidth = pathObject.PathSO.laneWidth;

            if (IsStartNodeOf(pathObject))
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartCenter);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.StartLeft);

                startVehiclePathNode = GetVehiclePathNodeFor(pathObject, PathNodeObject.OnPathPosition.StartNodeStartPath);
                endVehiclePathNode = GetVehiclePathNodeFor(pathObject, PathNodeObject.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                center = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndCenter);
                left = GetMeshEdjeFor(pathObject, MeshEdje.EdjePosition.EndLeft);

                startVehiclePathNode = GetVehiclePathNodeFor(pathObject, PathNodeObject.OnPathPosition.EndNodeStartPath);
                endVehiclePathNode = GetVehiclePathNodeFor(pathObject, PathNodeObject.OnPathPosition.EndNodeEndPath);
            }

            Vector3 centerPos = center.Position;
            Vector3 leftPos = left.Position;

            Vector3 leftDir = (leftPos - centerPos).normalized;

            Vector3 startPathPosition = centerPos + leftDir * laneWidth / 2;
            Vector3 endPathPosition = centerPos - leftDir * laneWidth / 2;

            startVehiclePathNode.transform.position = startPathPosition;
            endVehiclePathNode.transform.position = endPathPosition;

            startVehiclePathNode.transform.rotation = Quaternion.LookRotation(center.Direction);
            endVehiclePathNode.transform.rotation = Quaternion.LookRotation(center.Direction);

            // Handle widewalks
            VehiclePath path = pathObject as VehiclePath;
            VehiclePathSO vehiclePathSO = path.PathSO as VehiclePathSO;
            if (!vehiclePathSO.hasSidewalk) return;
            hasPathWithSidewalk = true;

            PedestrianPathNode startPedestrianPathNode;
            PedestrianPathNode endPedestrianPathNode;

            int width = vehiclePathSO.Width;
            int sidewalkWidth = vehiclePathSO.sidewalkWidth;

            if (IsStartNodeOf(path))
            {
                center = GetMeshEdjeFor(path, MeshEdje.EdjePosition.StartCenter);
                left = GetMeshEdjeFor(path, MeshEdje.EdjePosition.StartLeft);

                startPedestrianPathNode = GetPedestrianPathNodeFor(path, PathNodeObject.OnPathPosition.StartNodeStartPath);
                endPedestrianPathNode = GetPedestrianPathNodeFor(path, PathNodeObject.OnPathPosition.StartNodeEndPath);
            }
            else
            {
                center = GetMeshEdjeFor(path, MeshEdje.EdjePosition.EndCenter);
                left = GetMeshEdjeFor(path, MeshEdje.EdjePosition.EndLeft);

                startPedestrianPathNode = GetPedestrianPathNodeFor(path, PathNodeObject.OnPathPosition.EndNodeStartPath);
                endPedestrianPathNode = GetPedestrianPathNodeFor(path, PathNodeObject.OnPathPosition.EndNodeEndPath);
            }

            centerPos = center.Position;
            leftPos = left.Position;

            leftDir = (leftPos - centerPos).normalized;

            startPathPosition = centerPos + leftDir * (width / 2 - sidewalkWidth + sidewalkWidth / 2f);
            endPathPosition = centerPos - leftDir * (width / 2 - sidewalkWidth + sidewalkWidth / 2f);

            startPedestrianPathNode.transform.position = startPathPosition;
            endPedestrianPathNode.transform.position = endPathPosition;

            startPedestrianPathNode.transform.rotation = Quaternion.LookRotation(center.Direction);
            endPedestrianPathNode.transform.rotation = Quaternion.LookRotation(center.Direction);
        }
        public override void ConnectPathNodes()
        {
            VehiclePathNode thisPathStart;
            VehiclePathNode thisPathEnd;

            VehiclePathNode nextPathStart;

            foreach (PathObject connectedPath in ConnectedPaths)
            {
                if (IsStartNodeOf(connectedPath))
                {
                    thisPathStart = GetVehiclePathNodeFor(connectedPath, PathNodeObject.OnPathPosition.StartNodeStartPath);
                    thisPathEnd = GetVehiclePathNodeFor(connectedPath, PathNodeObject.OnPathPosition.StartNodeEndPath);
                }
                else
                {
                    thisPathStart = GetVehiclePathNodeFor(connectedPath, PathNodeObject.OnPathPosition.EndNodeStartPath);
                    thisPathEnd = GetVehiclePathNodeFor(connectedPath, PathNodeObject.OnPathPosition.EndNodeEndPath);
                }

                if (ConnectedPaths.Count == 1)
                {
                    thisPathEnd.AddPathNode(thisPathStart);
                }
                else
                {
                    foreach (PathObject nextConnectedPath in ConnectedPaths)
                    {
                        if (connectedPath == nextConnectedPath) continue;
                        if (IsStartNodeOf(nextConnectedPath))
                            nextPathStart = GetVehiclePathNodeFor(nextConnectedPath, PathNodeObject.OnPathPosition.StartNodeStartPath);
                        else
                            nextPathStart = GetVehiclePathNodeFor(nextConnectedPath, PathNodeObject.OnPathPosition.EndNodeStartPath);

                        thisPathEnd.AddPathNode(nextPathStart);
                    }
                }
            }

            // Handle sidewalks
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

                if (ConnectedPaths.Count == 1)
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
    }
}
