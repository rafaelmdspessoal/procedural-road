namespace Path.Entities.Pedestrian
{
    public class PedestrianPathNode : PathNodeObject
    {
        public override void Init(OnPathPosition pathPosition)
        {
            base.Init(pathPosition);
            transform.name = "Pedestrian " + pathPosition.ToString();
        }
    }
}