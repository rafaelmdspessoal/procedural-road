using Path.Entities.Pedestrian;
using Path.Entities.SO;
using UnityEngine;

namespace Path.Entities.Vehicle.SO
{
    [CreateAssetMenu(menuName = "Path/Vehicle")]
    public class VehiclePathSO : PathSO
    {
        public bool hasSidewalk = false;
        public int sidewalkWidth;

        public GameObject pedestrianPathNode;

        public override int Width => base.Width + (hasSidewalk ? (sidewalkWidth * 2) : 0);

        public PedestrianPathNode AddPedestrianPathNodeBetween(
            PedestrianPathNode startSidewalk,
            PedestrianPathNode endSidewalk,
            PedestrianNode pedestrianNode,
            Vector3 position)
        {
            // TODO Calculate node direction using tangent
            GameObject newPathNodeObject = Instantiate(pedestrianPathNode, position, Quaternion.identity, pedestrianNode.transform);
            PedestrianPathNode newSidewlakPathNode = newPathNodeObject.GetComponent<PedestrianPathNode>();

            startSidewalk.AddPathNode(newSidewlakPathNode);
            newSidewlakPathNode.AddPathNode(startSidewalk);

            endSidewalk.AddPathNode(newSidewlakPathNode);
            newSidewlakPathNode.AddPathNode(endSidewalk);

            pedestrianNode.ConnectToSidewalk(newSidewlakPathNode);

            return newSidewlakPathNode;
        }
        public override bool TryConnectToSidewalk(
            out VehiclePath pathToConnect,
            out PedestrianPathNode startPathNode,
            out PedestrianPathNode endPathNode,
            out Vector3 positionToConnect)
        {
            pathToConnect = default;
            startPathNode = default;
            endPathNode = default;
            positionToConnect = Vector3.negativeInfinity;
            // Debug.Log("Missing logic for hits!");
            return false;
        }
        public override bool TryGetPathPositions(out Vector3 hitPosition, out GameObject hitObject)
        {
            if (TryRaycastObject(out _, out VehicleNode nodeObj))
            {
                hitPosition = nodeObj.transform.position;
                hitObject = nodeObj.gameObject;
                return true;
            }

            if (TryRaycastObject(out hitPosition, out VehiclePath pathObj))
            {
                hitPosition = Bezier.GetClosestPointTo(pathObj, hitPosition);
                hitObject = pathObj.gameObject; ;
                return true;
            }

            return base.TryGetPathPositions(out hitPosition, out hitObject);
        }
    }
}
