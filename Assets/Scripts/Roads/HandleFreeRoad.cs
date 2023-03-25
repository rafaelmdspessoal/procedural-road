using UnityEngine;
using Rafael.Utils;
using Road.Utilities;
using Road.Obj;

namespace Road.Placement.Free {

    public class HandleFreeRoad : MonoBehaviour {

        private RoadPlacementManager roadPlacementManager;
        private InputManager inputManager;

        private void Start() {
            roadPlacementManager = RoadPlacementManager.Instance;
            inputManager = InputManager.Instance;
            inputManager.OnNodePlaced += InputManager_OnNodePlaced;
        }

        private void Update() {
            if (!roadPlacementManager.IsBuildingFreeStartNode() || !roadPlacementManager.IsBuilding()) return;
            int angleToSnap = roadPlacementManager.AngleToSnap;
            if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
                GameObject hitObj = hit.transform.gameObject;
                Vector3 hitPosition = RoadUtilities.GetHitPosition(hit.point, hitObj);
                Vector3 controlPosition;

                if (roadPlacementManager.IsBuildingStartNode())
                    roadPlacementManager.SetNodeGFXPosition(hitPosition);
                else {
                    
                    hitPosition = RoadUtilities.GetHitPosition(hitPosition, hitObj);

                    if (roadPlacementManager.StartNode.ConnectedRoads().Count > 0) {
                        controlPosition = RoadUtilities.GetProjectedPosition(
                            hitPosition,
                            roadPlacementManager.StartNode.Direction,
                            roadPlacementManager.StartPosition
                            );
                    } else {
                        if (roadPlacementManager.IsSnappingAngle && roadPlacementManager.CanSnap(hitObj)) {
                            // if we hit ground or a road
                            hitPosition = RoadUtilities.GetHitPositionWithSnapping(
                                hitPosition,
                                roadPlacementManager.StartNode,
                                angleToSnap
                                );
                        }
                        controlPosition = (roadPlacementManager.StartPosition + hitPosition) / 2;
                    }
                    
                    roadPlacementManager.SetNodeGFXPosition(hitPosition);
                    roadPlacementManager.ControlPosition = controlPosition;
                    roadPlacementManager.DisplayTemporaryMesh(roadPlacementManager.StartPosition, hitPosition, controlPosition);
                }
            }
        }

        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e) {
            if (!roadPlacementManager.IsBuildingFreeStartNode() || !roadPlacementManager.IsBuilding()) return;
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
                if (
                    roadPlacementManager.IsSnappingAngle && 
                    roadPlacementManager.CanSnap(obj) && 
                    roadPlacementManager.StartNode.ConnectedRoads().Count <= 0
                    ) {
                    // if we hit ground or a road
                    endPosition = RoadUtilities.GetHitPositionWithSnapping(endPosition, roadPlacementManager.StartNode, angleToSnap);
                }
                endPosition = RoadUtilities.GetHitPosition(endPosition, obj, true);

                roadPlacementManager.EndPosition = endPosition;
                roadPlacementManager.PlaceRoad();
                roadPlacementManager.SplitRoads();
                return;
            }
        }
    }
}
