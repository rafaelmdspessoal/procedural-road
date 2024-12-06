using Spline.Utils;
using UnityEngine;

namespace Spline.Placement {

    public class BuildingStraightSpline: IPlacementState {

        private SplinePlacementSystem placementSystem;
        private SplinePreviewSystem previewSystem;

        public BuildingStraightSpline(SplinePlacementSystem placementSystem)
        {
            this.placementSystem = placementSystem;
            previewSystem = SplinePreviewSystem.Instance;
        }

        public void UpdateState(Vector3 hitPosition, bool canBuildPath)
        {
            if (placementSystem.IsBuildingStartNode()) return;

            Vector3 startPosition = placementSystem.StartPosition; 
            Vector3 controlPosition = (startPosition + hitPosition) / 2;
            placementSystem.ControlPosition = controlPosition;

            Vector3[] path = Bezier.CalculateSplinePoints(
              startPosition,
              controlPosition,
              hitPosition,
              placementSystem.splineResolution,
              placementSystem.controlNodeRatio);

            previewSystem.DisplayTemporarySpline(path);
        }

        public void OnAction(Vector3 hitPosition, bool canBuildPath)
        {
            if (!canBuildPath) return;

            if (placementSystem.IsBuildingStartNode()) {
                placementSystem.StartPosition = hitPosition;
                placementSystem.BuildingState = SplinePlacementSystem.NodeBuildingState.EndNode;
                return;
            }

            if (placementSystem.IsBuildingEndNode())
            {
                Vector3 controlPosition = (placementSystem.StartPosition + hitPosition) / 2;

                placementSystem.ControlPosition = controlPosition;
                placementSystem.EndPosition = hitPosition;
                placementSystem.PlacePath();
                // pathPlacementSystem.SplitPath();
                return;
            }
        }
        public void StopPreviewDisplay()
        {
            previewSystem.StopPreview();
        }
    }
}
