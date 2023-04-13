using UnityEngine;
using Roads.Placement;
using Roads.Preview;
using Roads;

namespace Road.Placement.States {

    public class BuildingStraightRoad : IBuildingState {

        private RoadPlacementSystem roadPlacementSystem;
        private RoadPreviewSystem roadPreviewSystem;

        public BuildingStraightRoad(RoadPlacementSystem roadPlacementSystem)
        {
            this.roadPlacementSystem = roadPlacementSystem;
            roadPreviewSystem = RoadPreviewSystem.Instance;
        }

        public void UpdateState(Vector3 hitPosition, RoadObjectSO roadObjectSO, bool canBuildRoad)
        {
            if (roadPlacementSystem.IsBuildingStartNode()) return;

            Vector3 startPosition = roadPlacementSystem.StartPosition; 
            Vector3 controlPosition = (startPosition + hitPosition) / 2;
            roadPlacementSystem.ControlPosition = controlPosition;
            roadPreviewSystem.DisplayTemporaryMesh(
               startPosition,
               controlPosition,
               hitPosition,
               roadObjectSO.roadWidth,
               roadObjectSO.roadResolution,
               canBuildRoad);
        }

        public void OnAction(Vector3 hitPosition, bool canBuildRoad) {
            if (!canBuildRoad) return;

            if (roadPlacementSystem.IsBuildingStartNode()) {
                roadPlacementSystem.StartPosition = hitPosition;
                roadPlacementSystem.UpdateBuildingState(RoadPlacementSystem.NodeBuildingState.EndNode);
                return;
            }

            if (roadPlacementSystem.IsBuildingEndNode())
            {
                Vector3 controlPosition = (roadPlacementSystem.StartPosition + hitPosition) / 2;

                roadPlacementSystem.ControlPosition = controlPosition;
                roadPlacementSystem.EndPosition = hitPosition;
                roadPlacementSystem.PlaceRoad();
                roadPlacementSystem.SplitRoads();
                roadPlacementSystem.SetRoadsMesh();
                return;
            }
        }

        public void StopPreviewDisplay()
        {
            roadPreviewSystem.StopPreview();
        }
    }
}
