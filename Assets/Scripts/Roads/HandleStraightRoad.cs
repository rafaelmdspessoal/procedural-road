using UnityEngine;
using Rafael.Utils;
using Road.Utilities;
using Road.Obj;

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
            if (roadPlacementManager.IsBuildingStraightRoad) {
                int angleToSnap = roadPlacementManager.AngleToSnap;
                if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
                    GameObject hitObj = hit.transform.gameObject;
                    Vector3 hitPosition = RoadUtilities.GetHitPosition(hit.point, hitObj);

                    if (roadPlacementManager.IsBuildingStartNode())
                        roadPlacementManager.SetNodeGFXPosition(hitPosition);
                    else {
                        Vector3 startPosition = roadPlacementManager.StartPosition;
                        if (roadPlacementManager.IsSnappingAngle && roadPlacementManager.CanSnap(hitObj)) {
                            // if we hit ground or a road
                            hitPosition = RoadUtilities.GetHitPositionWithSnapping(
                                hitPosition,
                                roadPlacementManager.StartNode,
                                angleToSnap
                                );
                        }
                        hitPosition = RoadUtilities.GetHitPosition(hitPosition, hitObj);
                        hitPosition = roadPlacementManager.GetPositionForMinRoadLengh(hitPosition);
                        Vector3 controlPosition = (startPosition + hitPosition) / 2;

                        roadPlacementManager.SetNodeGFXPosition(hitPosition);
                        roadPlacementManager.ControlPosition = controlPosition;

                        // If road start check passes, also check for end
                        if (roadPlacementManager.CheckRoadAngleInRange(controlPosition - hitPosition))
                            roadPlacementManager.CheckRoadAngleInRange(controlPosition - hitPosition, hitObj, hitPosition);

                        roadPlacementManager.DisplayTemporaryMesh(startPosition, hitPosition, controlPosition);
                    }
                }
            }
        }

        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e) {
            if (!roadPlacementManager.CanBuildStraightRoad) return;
            int angleToSnap = roadPlacementManager.AngleToSnap;

            GameObject obj = e.obj;
            Debug.Log("Node Placed!");
            if (roadPlacementManager.IsBuildingStartNode()) {
                roadPlacementManager.StartPosition = RoadUtilities.GetHitPosition(e.position, obj, true);
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.EndNode);
                return;
            }

            if (roadPlacementManager.IsBuildingEndNode()) {
                Vector3 endPosition = e.position;
                if (roadPlacementManager.IsSnappingAngle && roadPlacementManager.CanSnap(obj)) {
                    // if we hit ground or a road
                    endPosition = RoadUtilities.GetHitPositionWithSnapping(endPosition, roadPlacementManager.StartNode, angleToSnap);
                }
                endPosition = RoadUtilities.GetHitPosition(endPosition, obj, true);
                endPosition = roadPlacementManager.GetPositionForMinRoadLengh(endPosition);
                roadPlacementManager.EndPosition = endPosition;
                roadPlacementManager.PlaceRoad();
                roadPlacementManager.SplitRoads();
                return;
            }
        }
    }
}
