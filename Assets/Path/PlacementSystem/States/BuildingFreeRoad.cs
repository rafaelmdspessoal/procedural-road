using UnityEngine;
using Path.Entities.SO;
using Path.PlacementSystem;
using Path.Preview;
using Path.Utilities;

namespace Path.Placement.States {

    public class BuildingFreePath : IBuildingState {

        private PathPlacementSystem pathPlacementSystem;
        private PathPreviewSystem pathPreviewSystem;

        public BuildingFreePath(PathPlacementSystem pathPlacementSystem)
        {
            this.pathPlacementSystem = pathPlacementSystem;
            pathPreviewSystem = PathPreviewSystem.Instance;
        }

        public void UpdateState(Vector3 hitPosition, PathSO pathObjectSO, bool canBuildPath)
        {
            if (pathPlacementSystem.IsBuildingStartNode()) return;

            Vector3 controlPosition;
            Vector3 startPosition = pathPlacementSystem.StartPosition;

            hitPosition = pathPlacementSystem.GetPositionForMinPathLengh(hitPosition);
            if (pathPlacementSystem.StartNode.HasConnectedPaths)
            {
                controlPosition = PathUtilities.GetProjectedPosition(
                    hitPosition,
                    pathPlacementSystem.StartNode.Direction,
                    startPosition
                    );
            }
            else
            {
                controlPosition = (startPosition + hitPosition) / 2;
            }

            pathPreviewSystem.DisplayTemporaryMesh(
              startPosition,
              controlPosition,
              hitPosition,
              pathObjectSO.width,
              pathObjectSO.resolution,
              canBuildPath);
        }

        public void StopPreviewDisplay()
        {
            pathPreviewSystem.StopPreview();
        }

        public void OnAction(Vector3 hitPosition, bool canBuildPath)
        {
            if (!canBuildPath) return;

            if (pathPlacementSystem.IsBuildingStartNode()) 
            {
                pathPlacementSystem.StartPosition = hitPosition;
                pathPlacementSystem.UpdateBuildingState(PathPlacementSystem.NodeBuildingState.EndNode);
                return;
            }

            if (pathPlacementSystem.IsBuildingEndNode())
            {
                Vector3 startPosition = pathPlacementSystem.StartPosition;
                Vector3 controlPosition;
                if (pathPlacementSystem.StartNode.HasConnectedPaths)
                {
                    controlPosition = PathUtilities.GetProjectedPosition(
                        hitPosition,
                        pathPlacementSystem.StartNode.Direction,
                        startPosition);
                }
                else
                {
                    controlPosition = (startPosition + hitPosition) / 2;
                }

                pathPlacementSystem.ControlPosition = controlPosition;
                pathPlacementSystem.EndPosition = hitPosition;
                pathPlacementSystem.PlacePath();
                pathPlacementSystem.SplitPath();
                return;
            }
        }
        public bool CanSnapAngle()
        {
            return false;
        }
    }
}
