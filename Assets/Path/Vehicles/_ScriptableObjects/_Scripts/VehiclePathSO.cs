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
    }
}
