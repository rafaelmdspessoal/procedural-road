using UnityEngine;
using Rafael.Utils;
using World;
using Roads.Placement;
using Roads.Preview;

namespace Road.Placement.States {
    public class BuildingCurvedRoad : IBuildingState {

        private RoadPlacementSystem roadPlacementSystem;
        private RoadPreviewSystem roadPreviewSystem;

        public BuildingCurvedRoad(RoadPlacementSystem roadPlacementSystem)
        {
            this.roadPlacementSystem = roadPlacementSystem;
            roadPreviewSystem = RoadPreviewSystem.Instance;
        }

        public void UpdateState(Vector3 hitPosition, RoadObjectSO roadObjectSO, bool canBuildRoad)
        {
            if (roadPlacementSystem.IsBuildingStartNode()) return;

            Vector3 startPosition = roadPlacementSystem.StartPosition;
            Vector3 controlPosition;
            if (roadPlacementSystem.IsBuildingControlNode())
            {
                controlPosition = (startPosition + hitPosition) / 2;
            }
            else
            {
                controlPosition = roadPlacementSystem.ControlPosition;
            }
            roadPlacementSystem.ControlPosition = controlPosition;
            roadPreviewSystem.DisplayTemporaryMesh(
               startPosition,
               controlPosition,
               hitPosition,
               roadObjectSO.roadWidth,
               roadObjectSO.roadResolution,
               canBuildRoad);
        }

        public void OnAction(Vector3 hitPosition, bool canBuildRoad)
        {
            if (!canBuildRoad) return;

            if (roadPlacementSystem.IsBuildingStartNode()) {
                roadPlacementSystem.StartPosition = hitPosition;
                roadPlacementSystem.UpdateBuildingState(RoadPlacementSystem.NodeBuildingState.ControlNode);
                return;
            }

            if (roadPlacementSystem.IsBuildingControlNode()) {
                roadPlacementSystem.ControlPosition = hitPosition;
                roadPlacementSystem.UpdateBuildingState(RoadPlacementSystem.NodeBuildingState.EndNode);
                return;
            }

            if (roadPlacementSystem.IsBuildingEndNode()) {
                roadPlacementSystem.EndPosition = hitPosition;
                roadPlacementSystem.PlaceRoad();
                roadPlacementSystem.SplitRoads();
                roadPlacementSystem.SetRoadsMesh();
                roadPlacementSystem.UpdateBuildingState(RoadPlacementSystem.NodeBuildingState.ControlNode);
                return;
            }
        }
        public void StopPreviewDisplay()
        {
            roadPreviewSystem.StopPreview();
        }
    }
}
