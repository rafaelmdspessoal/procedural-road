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
            if (!roadPlacementManager.IsBuildingStraightRoad() || !roadPlacementManager.IsBuilding()) return;
            int angleToSnap = roadPlacementManager.AngleToSnap;
            if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
                GameObject hitObj = hit.transform.gameObject;
                Vector3 hitPosition = RoadUtilities.GetHitPosition(hit.point, hitObj);

                if (roadPlacementManager.IsBuildingStartNode())
                    roadPlacementManager.SetNodeGFXPosition(hitPosition);
                else {
                    if (roadPlacementManager.IsSnappingAngle && roadPlacementManager.CanSnap(hitObj)) {
                        // if we hit ground or a road
                        hitPosition = RoadUtilities.GetHitPositionWithSnapping(
                            hitPosition, 
                            roadPlacementManager.StartNode, 
                            angleToSnap
                            );
                    }
                    Debug.Log("angleToSnap " + angleToSnap);
                    hitPosition = RoadUtilities.GetHitPosition(hitPosition, hitObj);

                    roadPlacementManager.SetNodeGFXPosition(hitPosition);
                    roadPlacementManager.ControlPosition = (roadPlacementManager.StartPosition + hitPosition) / 2;
                    roadPlacementManager.DisplayTemporaryMesh(roadPlacementManager.StartPosition, hitPosition, roadPlacementManager.ControlPosition);
                }
            }
        }

        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e) {
            if (!roadPlacementManager.IsBuildingStraightRoad() || !roadPlacementManager.IsBuilding()) return;
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

                roadPlacementManager.EndPosition = endPosition;
                roadPlacementManager.PlaceRoad();
                roadPlacementManager.SplitRoads();
                return;
            }
        }
    }
}
