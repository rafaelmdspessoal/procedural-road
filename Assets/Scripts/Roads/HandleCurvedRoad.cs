using UnityEngine;
using Rafael.Utils;
using Road.Utilities;

namespace Road.Placement.Curved {
    public class HandleCurvedRoad : MonoBehaviour {
        private RoadPlacementManager roadPlacementManager;
        private InputManager inputManager;

        private Vector3 controlPosition;
        private Vector3 startPosition;
        private Vector3 endPosition;

        private void Start() {
            roadPlacementManager = RoadPlacementManager.Instance;
            inputManager = InputManager.Instance;
            inputManager.OnNodePlaced += InputManager_OnNodePlaced;
        }

        private void Update() {
            if (!roadPlacementManager.IsBuildingCurvedRoad() || !roadPlacementManager.IsBuilding()) return;
            Debug.Log("Is Building Curved Road: " + roadPlacementManager.IsBuildingCurvedRoad());

            if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
                Vector3 hitPosition = RoadUtilities.GetHitPosition(hit.point, hit.transform.gameObject);

                roadPlacementManager.SetNodeGFXPosition(hitPosition);

                if (roadPlacementManager.IsBuildingStartNode()) return;

                if (roadPlacementManager.IsBuildingControlNode()) {
                    Vector3 tempControlPosition = (startPosition + hitPosition) / 2;
                    controlPosition = hitPosition;
                    roadPlacementManager.DisplayTemporaryMesh(startPosition, hitPosition, tempControlPosition);
                } else {
                    roadPlacementManager.DisplayTemporaryMesh(startPosition, hitPosition, controlPosition); 
                }
            }
        }


        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e) {
            if (!roadPlacementManager.IsBuildingCurvedRoad() || !roadPlacementManager.IsBuilding()) return;

            Debug.Log("Node Placed!");
            if (roadPlacementManager.IsBuildingStartNode()) {
                startPosition = RoadUtilities.GetHitPosition(e.position, e.obj, true);
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.ControlNode);
                return;
            }

            if (roadPlacementManager.IsBuildingControlNode()) {
                controlPosition = new Vector3(
                    e.position.x,
                    e.position.y + 0.1f,
                    e.position.z
                );
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.EndNode);
                return;
            }

            if (roadPlacementManager.IsBuildingEndNode()) {
                endPosition = RoadUtilities.GetHitPosition(e.position, e.obj, true);
                roadPlacementManager.PlaceRoad(startPosition, controlPosition, endPosition);
                roadPlacementManager.SplitRoads();
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.ControlNode);
                startPosition = endPosition;
                return;
            }
        }
    }
}
