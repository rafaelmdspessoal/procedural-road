namespace Path.Entities.Vehicle
{
    public class VehiclePathNode : PathNodeObject
    {
        public override void Init(OnPathPosition pathPosition)
        {
            base.Init(pathPosition);
            transform.name = "Vehicle " + pathPosition.ToString();
        }
    }
}