using UnityEngine;
using Path.Entities.SO;
using Path.PlacementSystem;
using Path.Preview;

namespace Path.Placement.States {
    public class BuildingCurvedPath : IBuildingState {

        private PathPlacementSystem pathPlacementSystem;
        private PathPreviewSystem pathPreviewSystem;

        public BuildingCurvedPath(PathPlacementSystem pathPlacementSystem)
        {
            this.pathPlacementSystem = pathPlacementSystem;
            pathPreviewSystem = PathPreviewSystem.Instance;
        }

        public void UpdateState(Vector3 hitPosition, PathSO pathObjectSO, bool canBuildPath)
        {
            if (pathPlacementSystem.IsBuildingStartNode()) return;

            Vector3 startPosition = pathPlacementSystem.StartPosition;
            Vector3 controlPosition;
            if (pathPlacementSystem.IsBuildingControlNode())
            {
                controlPosition = (startPosition + hitPosition) / 2;
            }
            else
            {
                controlPosition = pathPlacementSystem.ControlPosition;
            }
            pathPlacementSystem.ControlPosition = controlPosition;
            pathPreviewSystem.DisplayTemporaryMesh(
               startPosition,
               controlPosition,
               hitPosition,
               pathObjectSO.width,
               pathObjectSO.resolution,
               canBuildPath);
        }

        public void OnAction(Vector3 hitPosition, bool canBuildPath)
        {
            if (!canBuildPath) return;

            if (pathPlacementSystem.IsBuildingStartNode()) {
                pathPlacementSystem.StartPosition = hitPosition;
                pathPlacementSystem.UpdateBuildingState(PathPlacementSystem.NodeBuildingState.ControlNode);
                return;
            }

            if (pathPlacementSystem.IsBuildingControlNode()) {
                pathPlacementSystem.ControlPosition = hitPosition;
                pathPlacementSystem.UpdateBuildingState(PathPlacementSystem.NodeBuildingState.EndNode);
                return;
            }

            if (pathPlacementSystem.IsBuildingEndNode()) {
                pathPlacementSystem.EndPosition = hitPosition;
                pathPlacementSystem.PlacePath();
                pathPlacementSystem.SplitPath();
                pathPlacementSystem.UpdateBuildingState(PathPlacementSystem.NodeBuildingState.ControlNode);
                return;
            }
        }
        public void StopPreviewDisplay()
        {
            pathPreviewSystem.StopPreview();
        }
    }
}
