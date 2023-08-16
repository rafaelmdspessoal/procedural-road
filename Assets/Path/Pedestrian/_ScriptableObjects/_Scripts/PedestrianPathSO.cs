using Path.Entities.SO;
using Path.Entities.Vehicle;
using Path.Entities.Vehicle.SO;
using Rafael.Utils;
using System.Drawing.Printing;
using System.IO;
using UnityEngine;

namespace Path.Entities.Pedestrian.SO {
    [CreateAssetMenu(menuName = "Path/Pedestrian")]
    public class PedestrianPathSO : PathSO
    {
        public override int Width => base.Width;

        public override bool TryGetPathPositions(out Vector3 hitPosition, out GameObject hitObject)
        {
            if (TryRaycastObject(out _, out PedestrianNode nodeObj))
            {
                hitPosition = nodeObj.transform.position;
                hitObject = nodeObj.gameObject;
                return true;
            }

            if (TryRaycastObject(out hitPosition, out PedestrianPath pathObj))
            {
                hitPosition = Bezier.GetClosestPointTo(pathObj, hitPosition);
                hitObject = pathObj.gameObject;
                return true;
            }

            return base.TryGetPathPositions(out hitPosition, out hitObject);
        }

        public override bool TryConnectToSidewalk(
            out VehiclePath pathToConnect,
            out PedestrianPathNode startPathNode,
            out PedestrianPathNode endPathNode,
            out Vector3 positionToConnect,
            NodeObject nodeObject = null)
        {
            startPathNode = default;
            endPathNode = default;
            positionToConnect = Vector3.negativeInfinity;


            if (TryRaycastObject(out Vector3 hitPosition, out pathToConnect, nodeObject))
            {
                VehiclePathSO vehiclePathSO = pathToConnect.PathSO as VehiclePathSO;
                if ((vehiclePathSO).hasSidewalk)
                {
                    PedestrianPathNode startLeftPathNode;
                    PedestrianPathNode startRightPathNode;

                    PedestrianPathNode endLeftPathNode;
                    PedestrianPathNode endRightPathNode;

                    VehicleNode startNode = pathToConnect.StartNode as VehicleNode;
                    VehicleNode endNode = pathToConnect.EndNode as VehicleNode;

                    startLeftPathNode = startNode.GetPedestrianPathNodeFor(pathToConnect, PathNodeObject.OnPathPosition.StartNodeStartPath);
                    startRightPathNode = startNode.GetPedestrianPathNodeFor(pathToConnect, PathNodeObject.OnPathPosition.StartNodeEndPath);

                    endLeftPathNode = endNode.GetPedestrianPathNodeFor(pathToConnect, PathNodeObject.OnPathPosition.EndNodeStartPath);
                    endRightPathNode = endNode.GetPedestrianPathNodeFor(pathToConnect, PathNodeObject.OnPathPosition.EndNodeEndPath);

                    Vector3 closestLeftPoint = GetClosestPoint(hitPosition, startLeftPathNode, endRightPathNode);
                    Vector3 closestRightPoint = GetClosestPoint(hitPosition, startRightPathNode, endLeftPathNode);

                    float leftDistance = Vector3.Distance(hitPosition, closestLeftPoint);
                    float rightDistance = Vector3.Distance(hitPosition, closestRightPoint);

                    if (leftDistance < rightDistance)
                    {
                        positionToConnect = closestLeftPoint;
                        startPathNode = startLeftPathNode;
                        endPathNode = endRightPathNode;
                    }
                    else
                    {
                        positionToConnect = closestRightPoint;
                        startPathNode = startRightPathNode;
                        endPathNode = endLeftPathNode;
                    }
                    Vector3 ups = Vector3.up * 0.1f;

                    Debug.DrawLine(hitPosition + ups, positionToConnect + ups, Color.magenta);
                    return true;
                }
            }

            return false;
        }

        private static Vector3 GetClosestPoint(Vector3 hitPosition, PedestrianPathNode startPathNode, PedestrianPathNode endPathNode)
        {
            RafaelUtils.LineLineIntersection(
                out Vector3 intersection,
                startPathNode.Position,
                startPathNode.Direction,
                endPathNode.Position,
                endPathNode.Direction);

            Vector3 closestPoint = Bezier.GetClosestPointTo(
                startPathNode.Position,
                endPathNode.Position,
                intersection,
                hitPosition);
            return closestPoint;
        }
    }
}
