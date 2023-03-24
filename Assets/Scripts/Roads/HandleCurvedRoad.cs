using UnityEngine;
using Rafael.Utils;
using Road.Utilities;
using World;

namespace Road.Placement.Curved {
    public class HandleCurvedRoad : MonoBehaviour {
        private RoadPlacementManager roadPlacementManager;
        private InputManager inputManager;

        private void Start() {
            roadPlacementManager = RoadPlacementManager.Instance;
            inputManager = InputManager.Instance;
            inputManager.OnNodePlaced += InputManager_OnNodePlaced;
        }

        private void Update() {
            if (!roadPlacementManager.IsBuildingCurvedRoad() || !roadPlacementManager.IsBuilding()) return;

            if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
                Vector3 hitPosition = RoadUtilities.GetHitPosition(hit.point, hit.transform.gameObject);

                if (roadPlacementManager.IsBuildingStartNode())
                    roadPlacementManager.SetNodeGFXPosition(hitPosition);
                else if (roadPlacementManager.IsBuildingControlNode()) {
                    if (roadPlacementManager.AngleSnap && hit.transform.gameObject.TryGetComponent(out Ground _)) {
                        // Only tries to snap if we hit ground
                        hitPosition = RoadUtilities.GetHitPositionWithSnapping(hitPosition, roadPlacementManager.StartNode, 15);
                    }
                    roadPlacementManager.SetNodeGFXPosition(hitPosition);
                    Vector3 tempControlPosition = (roadPlacementManager.StartPosition + hitPosition) / 2;
                    roadPlacementManager.ControlPosition = hitPosition;
                    roadPlacementManager.DisplayTemporaryMesh(roadPlacementManager.StartPosition, hitPosition, tempControlPosition);
                } else {
                    if (roadPlacementManager.AngleSnap && hit.transform.gameObject.TryGetComponent(out Ground _)) {
                        // Only tries to snap if we hit ground
                        hitPosition = RoadUtilities.GetHitPositionWithSnapping(hitPosition, roadPlacementManager.StartPosition, roadPlacementManager.ControlPosition, 15);
                    }
                    roadPlacementManager.SetNodeGFXPosition(hitPosition);
                    roadPlacementManager.DisplayTemporaryMesh(roadPlacementManager.StartPosition, hitPosition, roadPlacementManager.ControlPosition);
                }
            }
        }

        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e) {
            if (!roadPlacementManager.IsBuildingCurvedRoad() || !roadPlacementManager.IsBuilding()) return;

            Debug.Log("Node Placed!");
            if (roadPlacementManager.IsBuildingStartNode()) {
                roadPlacementManager.StartPosition = RoadUtilities.GetHitPosition(e.position, e.obj, true);
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.ControlNode);
                return;
            }

            if (roadPlacementManager.IsBuildingControlNode()) {
                Vector3 controlPosition = new(
                    e.position.x,
                    e.position.y + 0.1f,
                    e.position.z
                );
                if (roadPlacementManager.AngleSnap && e.obj.TryGetComponent(out Ground _)) {
                    // Only tries to snap if we hit ground
                    controlPosition = RoadUtilities.GetHitPositionWithSnapping(controlPosition, roadPlacementManager.StartNode, 15);
                }
                roadPlacementManager.ControlPosition = controlPosition;
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.EndNode);
                return;
            }

            if (roadPlacementManager.IsBuildingEndNode()) {
                Vector3 endPosition = RoadUtilities.GetHitPosition(e.position, e.obj, true);
                if (roadPlacementManager.AngleSnap && e.obj.TryGetComponent(out Ground _)) {
                    // Only tries to snap if we hit ground
                    endPosition = RoadUtilities.GetHitPositionWithSnapping(endPosition, roadPlacementManager.StartPosition, roadPlacementManager.ControlPosition, 15);
                }
                roadPlacementManager.EndPosition = endPosition;
                roadPlacementManager.PlaceRoad();
                roadPlacementManager.SplitRoads();
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.ControlNode);
                return;
            }
        }
    }
}
