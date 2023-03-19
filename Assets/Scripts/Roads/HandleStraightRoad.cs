using UnityEngine;
using Road.Manager;
using Rafael.Utils;
using Road.Utilities;
using Road.Obj;
using Road.NodeObj;

namespace Road.Placement.Straight {

    public class HandleStraightRoad : MonoBehaviour {

        private RoadManager roadManager;
        private RoadPlacementManager roadPlacementManager;
        private InputManager inputManager;

        private GameObject nodeGFX;

        private Vector3 controlPosition;
        private Vector3 startPosition;
        private Vector3 endPosition;

        private void Start() {
            roadManager = RoadManager.Instance;
            roadPlacementManager = RoadPlacementManager.Instance;
            inputManager = InputManager.Instance;
            inputManager.OnNodePlaced += InputManager_OnNodePlaced;
        }

        private void Update() {
            if (!roadPlacementManager.IsBuildingStraightRoad() || !roadPlacementManager.IsBuilding()) return;
            Debug.Log("Is Building Straight Road: " + roadPlacementManager.IsBuildingStraightRoad());

            if (nodeGFX == null) nodeGFX = RoadUtilities.CreateNodeGFX(roadPlacementManager.GetRoadObjectSO());
            else nodeGFX.SetActive(true);

            if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
                GameObject hitObject = hit.transform.gameObject;
                Vector3 hitPosition = GetHitPosition(hit.point, hitObject);

                nodeGFX.transform.position = hitPosition;

                if (roadPlacementManager.GetBuildingState() == RoadPlacementManager.BuildingState.StartNode) return;

                controlPosition = (startPosition + hitPosition) / 2;
                roadPlacementManager.DisplayTemporaryMesh(startPosition, hitPosition, controlPosition);
            }
        }


        private void InputManager_OnNodePlaced(object sender, InputManager.OnObjectHitedEventArgs e) {
            if (!roadPlacementManager.IsBuildingStraightRoad() || !roadPlacementManager.IsBuilding()) return;

            Debug.Log("Node Placed!");
            if (roadPlacementManager.GetBuildingState() == RoadPlacementManager.BuildingState.StartNode) {
                startPosition = GetHitPosition(e.position, e.obj, true);
                roadPlacementManager.UpdateBuildingState(RoadPlacementManager.BuildingState.EndNode);
                return;
            }

            if (roadPlacementManager.GetBuildingState() == RoadPlacementManager.BuildingState.EndNode) {
                endPosition = GetHitPosition(e.position, e.obj, true);
                roadPlacementManager.OnNodesPlaced?.Invoke(this, new RoadPlacementManager.OnNodesPlacedEventArgs {
                    startNodePosition = startPosition,
                    controlNodePosition = controlPosition,
                    endNodePosition = endPosition,
                    roadObjectSO = roadPlacementManager.GetRoadObjectSO()
                });
                startPosition = endPosition;
                return;
            }
        }

        private Vector3 GetHitPosition(Vector3 hitPosition, GameObject hitObject, bool splitRoad = false) {
            Vector3 targetPosition = hitPosition;
            if (hitObject.TryGetComponent(out Node _)) {
                return hitObject.transform.position;
            } else if (hitObject.TryGetComponent(out RoadObject roadObject)) {
                targetPosition = Bezier.GetClosestPointTo(roadObject, hitPosition);
                if (splitRoad)
                    roadManager.AddRoadToSplit(targetPosition, roadObject);
                return targetPosition;
            }
            return new Vector3(
                targetPosition.x,
                targetPosition.y + 0.1f,
                targetPosition.z
            );
        }
    }
}
