using UnityEngine;
using Roads.Utilities;
using Roads.Placement;
using Roads.Preview;

namespace Road.Placement.States {

    public class BuildingFreeRoad : IBuildingState {

        private RoadPlacementSystem roadPlacementSystem;
        private RoadPreviewSystem roadPreviewSystem;

        public BuildingFreeRoad(RoadPlacementSystem roadPlacementSystem)
        {
            this.roadPlacementSystem = roadPlacementSystem;
            roadPreviewSystem = RoadPreviewSystem.Instance;
        }

        public void UpdateState(Vector3 hitPosition, RoadObjectSO roadObjectSO, bool canBuildRoad)
        {
            if (roadPlacementSystem.IsBuildingStartNode()) return;

            Vector3 controlPosition;
            Vector3 startPosition = roadPlacementSystem.StartPosition;

            hitPosition = roadPlacementSystem.GetPositionForMinRoadLengh(hitPosition);
            if (roadPlacementSystem.StartNode.HasConnectedRoads)
            {
                controlPosition = RoadUtilities.GetProjectedPosition(
                    hitPosition,
                    roadPlacementSystem.StartNode.Direction,
                    startPosition
                    );
            }
            else
            {
                controlPosition = (startPosition + hitPosition) / 2;
            }

            roadPreviewSystem.DisplayTemporaryMesh(
              startPosition,
              controlPosition,
              hitPosition,
              roadObjectSO.roadWidth,
              roadObjectSO.roadResolution,
              canBuildRoad);
        }

        public void StopPreviewDisplay()
        {
            roadPreviewSystem.StopPreview();
        }

        public void OnAction(Vector3 hitPosition, bool canBuildRoad)
        {
            if (!canBuildRoad) return;

            if (roadPlacementSystem.IsBuildingStartNode()) 
            {
                roadPlacementSystem.StartPosition = hitPosition;
                roadPlacementSystem.UpdateBuildingState(RoadPlacementSystem.NodeBuildingState.EndNode);
                return;
            }

            if (roadPlacementSystem.IsBuildingEndNode())
            {
                Vector3 startPosition = roadPlacementSystem.StartPosition;
                Vector3 controlPosition;
                if (roadPlacementSystem.StartNode.HasConnectedRoads)
                {
                    controlPosition = RoadUtilities.GetProjectedPosition(
                        hitPosition,
                        roadPlacementSystem.StartNode.Direction,
                        startPosition);
                }
                else
                {
                    controlPosition = (startPosition + hitPosition) / 2;
                }

                roadPlacementSystem.ControlPosition = controlPosition;
                roadPlacementSystem.EndPosition = hitPosition;
                roadPlacementSystem.PlaceRoad();
                roadPlacementSystem.SplitRoads();
                roadPlacementSystem.SetRoadsMesh();
                return;
            }
        }
    }
}
