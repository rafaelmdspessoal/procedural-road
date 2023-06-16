using Path.Entities;
using Path.Entities.Pedestrian;
using Path.Entities.Vehicle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Path.AI
{
    public class PathFindingManager : MonoBehaviour
    {
        public static PathFindingManager Instance { get; private set; }

        private List<PedestrianPathNode> pedestrianPathNodeList = new();
        private List<VehiclePathNode> vehiclePathNodeList = new();

        public List<PedestrianPathNode> PedestrianPathNodes { get { return pedestrianPathNodeList; } }
        public List<VehiclePathNode> VehiclePathNodes { get { return vehiclePathNodeList; } }


        private void Awake()
        {
            Instance = this;
        }

        public void AddPedestrianPathNode(PedestrianPathNode node)
        {
            if (!pedestrianPathNodeList.Contains(node))
                pedestrianPathNodeList.Add(node);
        }
        public void RemovePedestrianPathNode(PedestrianPathNode node)
        {
            if (pedestrianPathNodeList.Contains(node))
                pedestrianPathNodeList.Remove(node);
        }

        public void AddVehiclePathNode(VehiclePathNode node)
        {
            if (!vehiclePathNodeList.Contains(node))
                vehiclePathNodeList.Add(node);
        }
        public void RemoveVehiclePathNode(VehiclePathNode node)
        {
            if (vehiclePathNodeList.Contains(node))
                vehiclePathNodeList.Remove(node);
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var item in pedestrianPathNodeList)
            {
                List<PathNodeObject> connectedNodes = item.GetConnectedNodes();
                foreach (var node in connectedNodes)
                {
                    Gizmos.DrawLine(item.Position, node.Position);
                }
            }
            Gizmos.color = Color.green;
            foreach (var item in vehiclePathNodeList)
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
