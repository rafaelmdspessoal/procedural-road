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

                if (roadPlacementManager.IsBuildingStartNode())
                    roadPlacementManager.SetNodeGFXPosition(hitPosition);
                else {
                    if (roadPlacementManager.AngleSnap) {
                        hitPosition = RoadUtilities.GetHitPositionWithSnapping(hitPosition, roadPlacementManager.StartNode, 15);
                        roadPlacementManager.SetNodeGFXPosition(hitPosition);
                    }
                    roadPlacementManager.ControlPosition = (roadPlacementManager.StartPosition + hitPosition) / 2;
                    roadPlacementManager.DisplayTemporaryMesh(roadPlacementManager.StartPosition, hitPosition, roadPlacementManager.ControlPosition);
                }
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
                Vector3 endPosition = RoadUtilities.GetHitPosition(e.position, e.obj, true);
                if (roadPlacementManager.AngleSnap) 
                    endPosition = RoadUtilities.GetHitPositionWithSnapping(endPosition, roadPlacementManager.StartNode, 15);

                roadPlacementManager.EndPosition = endPosition;
                roadPlacementManager.PlaceRoad();
                roadPlacementManager.SplitRoads();
                return;
            }
        }
    }
}
