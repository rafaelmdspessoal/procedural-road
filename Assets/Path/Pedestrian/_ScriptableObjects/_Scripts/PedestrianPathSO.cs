using Path.Entities.SO;
using UnityEngine;

namespace Path.Entities.Pedestrian.SO {
    [CreateAssetMenu(menuName = "Path/Pedestrian")]
    public class PedestrianPathSO : PathSO
    {
        public override int Width => base.Width;
    }
}
