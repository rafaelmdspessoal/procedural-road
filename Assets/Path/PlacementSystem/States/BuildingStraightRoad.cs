using UnityEngine;
using Path.Entities.SO;
using Path.PlacementSystem;
using Path.Preview;

namespace Path.Placement.States {

    public class BuildingStraightPath : IBuildingState {

        private PathPlacementSystem pathPlacementSystem;
        private PathPreviewSystem pathPreviewSystem;

        public BuildingStraightPath(PathPlacementSystem pathPlacementSystem)
        {
            this.pathPlacementSystem = pathPlacementSystem;
            pathPreviewSystem = PathPreviewSystem.Instance;
        }

        public void UpdateState(Vector3 hitPosition, PathSO pathObjectSO, bool canBuildPath)
        {
            if (pathPlacementSystem.IsBuildingStartNode()) return;

            Vector3 startPosition = pathPlacementSystem.StartPosition; 
            Vector3 controlPosition = (startPosition + hitPosition) / 2;
            pathPlacementSystem.ControlPosition = controlPosition;
            pathPreviewSystem.DisplayTemporaryMesh(
               startPosition,
               controlPosition,
               hitPosition,
               pathObjectSO.width,
               pathObjectSO.resolution,
               canBuildPath);
        }

        public void OnAction(Vector3 hitPosition, bool canBuildPath) {
            if (!canBuildPath) return;

            if (pathPlacementSystem.IsBuildingStartNode()) {
                pathPlacementSystem.StartPosition = hitPosition;
                pathPlacementSystem.UpdateBuildingState(PathPlacementSystem.NodeBuildingState.EndNode);
                return;
            }

            if (pathPlacementSystem.IsBuildingEndNode())
            {
                Vector3 controlPosition = (pathPlacementSystem.StartPosition + hitPosition) / 2;

                pathPlacementSystem.ControlPosition = controlPosition;
                pathPlacementSystem.EndPosition = hitPosition;
                pathPlacementSystem.PlacePath();
                pathPlacementSystem.SplitPath();
                return;
            }
        }

        public void StopPreviewDisplay()
        {
            pathPreviewSystem.StopPreview();
        }
    }
}
