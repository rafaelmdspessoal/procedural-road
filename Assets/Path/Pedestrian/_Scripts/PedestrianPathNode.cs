using Path.AI;

namespace Path.Entities.Pedestrian
{
    public class PedestrianPathNode : PathNodeObject
    {        private void Start()
        {
            PathFindingManager.Instance.AddPedestrianPathNode(this);
        }
        public override void Init(OnPathPosition pathPosition)
        {
            base.Init(pathPosition);
            transform.name = "Pedestrian " + pathPosition.ToString();
        }
        protected override void OnDestroy()
        {
            PathFindingManager.Instance.RemovePedestrianPathNode(this);
            base.OnDestroy();
        }
    }
}