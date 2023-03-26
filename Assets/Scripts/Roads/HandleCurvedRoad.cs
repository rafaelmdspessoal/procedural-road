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
            if (roadPlacementManager.IsBuildingCurvedRoad) {
                if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
                    GameObject hitObj = hit.transform.gameObject;
                    Vector3 hitPosition = hit.point;

                    if (roadPlacementManager.IsBuildingStartNode()) {
                        hitPosition = RoadUtilities.GetHitPosition(hitPosition, hitObj);
                    } else if (roadPlacementManager.IsBuildingControlNode()) {
                        Vector3 startPosition = roadPlacementManager.StartPosition;
                        if (roadPlacementManager.IsSnappingAngle && hitObj.TryGetComponent(out Ground _)) {
                            // Only tries to snap if we hit ground
                            hitPosition = RoadUtilities.GetHitPositionWithSnapping(hitPosition, roadPlacementManager.StartNode, 15);
                        }
                        hitPosition = RoadUtilities.GetHitPosition(hitPosition, hitObj);
                        Vector3 tempControlPosition = (startPosition + hitPosition) / 2;
                        roadPlacementManager.ControlPosition = hitPosition;

                        // Don't pass hit object because we don't want to validate angle for a control node
                        roadPlacementManager.CheckRoadAngleInRange(startPosition - tempControlPosition);
                        roadPlacementManager.DisplayTemporaryMesh(startPosition, hitPosition, tempControlPosition);
                    } else {
                        Vector3 startPosition = roadPlacementManager.StartPosition;
                        Vector3 controlPosition = roadPlacementManager.ControlPosition;
                        if (roadPlacementManager.IsSnappingAngle && roadPlacementManager.CanSnap(hitObj)) {
                            // if we hit ground or a road
                            hitPosition = RoadUtilities.GetHitPositionWithSnapping(hitPosition, startPosition, controlPosition, 15);
                        }
                        hitPosition = RoadUtilities.GetHitPosition(hitPosition, hitObj);
                        roadPlacementManager.CheckRoadAngleInRange(controlPosition - hitPosition, hitObj, hitPosition);
                        roadPlacementManager.DisplayTemporaryMesh(startPosition, hitPosition, controlPosition);
                    }
                    roadPlacementManager.SetNodeGFXPosition(hitPosition);
                }
            }
        }

        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e) {
            if (!roadPlacementManager.CanBuildCurvedRoad) return;

            GameObject obj = e.obj;
            Debug.Log("Node Placed!");
            if (roadPlacementManager.IsBuildingStartNode()) {
                roadPlacementManager.StartPosition = RoadUtilities.GetHitPosition(e.position, obj, true);
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.ControlNode);
                return;
            }

            if (roadPlacementManager.IsBuildingControlNode()) {
                Vector3 controlPosition = new(
                    e.position.x,
                    e.position.y + 0.1f,
                    e.position.z
                );
                if (roadPlacementManager.IsSnappingAngle && obj.TryGetComponent(out Ground _)) {
                    // Only tries to snap if we hit ground
                    controlPosition = RoadUtilities.GetHitPositionWithSnapping(controlPosition, roadPlacementManager.StartNode, 15);
                }
                roadPlacementManager.ControlPosition = controlPosition;
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.EndNode);
                return;
            }

            if (roadPlacementManager.IsBuildingEndNode()) {
                Vector3 endPosition = e.position;
                if (roadPlacementManager.IsSnappingAngle && roadPlacementManager.CanSnap(obj)) {
                    // if we hit ground or a road
                    endPosition = RoadUtilities.GetHitPositionWithSnapping(endPosition, roadPlacementManager.StartPosition, roadPlacementManager.ControlPosition, 15);
                }
                endPosition = RoadUtilities.GetHitPosition(endPosition, obj, true);

                roadPlacementManager.EndPosition = endPosition;
                roadPlacementManager.PlaceRoad();
                roadPlacementManager.SplitRoads();
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.ControlNode);
                return;
            }
        }
    }
}
