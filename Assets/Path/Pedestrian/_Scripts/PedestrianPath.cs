using Path.Entities;
using Path.Entities.Vehicle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Path.Entities.Pedestrian
{
    public class PedestrianPath : PathObject
    {
        protected override void ConnectPathNodes()
        {
            base.ConnectPathNodes();

            PedestrianPathNode startNodeStartPath = startNode.GetPedestrianPathNodeFor(this, PathNodeObject.OnPathPosition.StartNodeStartPath);
            PedestrianPathNode startNodeEndPath = startNode.GetPedestrianPathNodeFor(this, PathNodeObject.OnPathPosition.StartNodeEndPath);

            PedestrianPathNode endNodeStartPath = endNode.GetPedestrianPathNodeFor(this, PathNodeObject.OnPathPosition.EndNodeStartPath);
            PedestrianPathNode endNodeEndPath = endNode.GetPedestrianPathNodeFor(this, PathNodeObject.OnPathPosition.EndNodeEndPath);

            startNodeStartPath.AddPathNode(endNodeEndPath);
            endNodeStartPath.AddPathNode(startNodeEndPath);
        }
    }
}
