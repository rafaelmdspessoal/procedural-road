using Path.Entities.Pedestrian;
using Path.Entities.Vehicle.SO;

namespace Path.Entities.Vehicle
{
    public class VehiclePath : PathObject
    {
        protected override void ConnectPathNodes()
        {
            VehiclePathNode startNodeStartPath = startNode.GetVehiclePathNodeFor(this, PathNodeObject.OnPathPosition.StartNodeStartPath);
            VehiclePathNode startNodeEndPath = startNode.GetVehiclePathNodeFor(this, PathNodeObject.OnPathPosition.StartNodeEndPath);

            VehiclePathNode endNodeStartPath = endNode.GetVehiclePathNodeFor(this, PathNodeObject.OnPathPosition.EndNodeStartPath);
            VehiclePathNode endNodeEndPath = endNode.GetVehiclePathNodeFor(this, PathNodeObject.OnPathPosition.EndNodeEndPath);

            startNodeStartPath.AddPathNode(endNodeEndPath);
            endNodeStartPath.AddPathNode(startNodeEndPath);

            // Handle sidewalks
            VehiclePathSO vehiclePath = PathSO as VehiclePathSO;
            if (!vehiclePath.hasSidewalk) return;

            VehicleNode startVehicleNode = startNode as VehicleNode;
            VehicleNode endVehicleNode = endNode as VehicleNode;

            PedestrianPathNode pedestrianStartNodeStartPath = startVehicleNode.GetPedestrianPathNodeFor(
                this,
                PathNodeObject.OnPathPosition.StartNodeStartPath);
            PedestrianPathNode pedestrianStartNodeEndPath = startVehicleNode.GetPedestrianPathNodeFor(
                this,
                PathNodeObject.OnPathPosition.StartNodeEndPath);

            PedestrianPathNode pedestrianEndNodeStartPath = endVehicleNode.GetPedestrianPathNodeFor(
                this,
                PathNodeObject.OnPathPosition.EndNodeStartPath);
            PedestrianPathNode pedestrianEndNodeEndPath = endVehicleNode.GetPedestrianPathNodeFor(
                this,
                PathNodeObject.OnPathPosition.EndNodeEndPath);

            pedestrianStartNodeEndPath.AddPathNode(pedestrianEndNodeStartPath);
            pedestrianEndNodeStartPath.AddPathNode(pedestrianStartNodeEndPath);

            pedestrianStartNodeStartPath.AddPathNode(pedestrianEndNodeEndPath);
            pedestrianEndNodeEndPath.AddPathNode(pedestrianStartNodeStartPath);
        }
    }
}