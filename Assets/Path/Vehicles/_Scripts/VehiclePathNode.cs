using Path.AI;

namespace Path.Entities.Vehicle
{
    public class VehiclePathNode : PathNodeObject
    {
        private void Start()
        {
            PathFindingManager.Instance.AddVehiclePathNode(this);
        }
        public override void Init(OnPathPosition pathPosition)
        {
            base.Init(pathPosition);
            transform.name = "Vehicle " + pathPosition.ToString();
        }
        protected override void OnDestroy()
        {
            PathFindingManager.Instance.RemoveVehiclePathNode(this);
            base.OnDestroy();
        }
    }
}