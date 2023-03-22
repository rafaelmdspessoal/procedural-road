using UnityEngine;
using Rafael.Utils;
using Road.Utilities;

namespace Road.Placement.Straight {

    public class HandleStraightRoad : MonoBehaviour {

        private RoadPlacementManager roadPlacementManager;
        private InputManager inputManager;

        private void Start() {
            roadPlacementManager = RoadPlacementManager.Instance;
            inputManager = InputManager.Instance;
            inputManager.OnNodePlaced += InputManager_OnNodePlaced;
        }

        private void Update() {
            if (!roadPlacementManager.IsBuildingStraightRoad() || !roadPlacementManager.IsBuilding()) return;
            Debug.Log("Is Building Straight Road: " + roadPlacementManager.IsBuildingStraightRoad());

            if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
                Vector3 hitPosition = RoadUtilities.GetHitPosition(hit.point, hit.transform.gameObject);

                roadPlacementManager.SetNodeGFXPosition(hitPosition);

                if (roadPlacementManager.IsBuildingStartNode()) return;

                // roadPlacementManager.GetPositionWithAngleSnap(hitPosition, )

                roadPlacementManager.ControlPosition = (roadPlacementManager.StartPosition + hitPosition) / 2;
                roadPlacementManager.DisplayTemporaryMesh(roadPlacementManager.StartPosition, hitPosition, roadPlacementManager.ControlPosition);
            }
        }

        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e) {
            if (!roadPlacementManager.IsBuildingStraightRoad() || !roadPlacementManager.IsBuilding()) return;

            Debug.Log("Node Placed!");
            if (roadPlacementManager.IsBuildingStartNode()) {
                roadPlacementManager.StartPosition = RoadUtilities.GetHitPosition(e.position, e.obj, true);
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.EndNode);
                return;
            }

            if (roadPlacementManager.IsBuildingEndNode()) {
                roadPlacementManager.EndPosition = RoadUtilities.GetHitPosition(e.position, e.obj, true);
                roadPlacementManager.PlaceRoad();
                roadPlacementManager.SplitRoads();
                return;
            }
        }
    }
}
