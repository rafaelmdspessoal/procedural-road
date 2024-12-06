using UnityEngine;
using Spline.Utils;

namespace Spline.Placement {
    public class BuildingCurvedSpline: IPlacementState
    {

        private SplinePlacementSystem placementSystem;
        private SplinePreviewSystem previewSystem;

        public BuildingCurvedSpline(SplinePlacementSystem placementSystem)
        {
            this.placementSystem = placementSystem;
            previewSystem = SplinePreviewSystem.Instance;
        }

        public void UpdateState(Vector3 hitPosition, bool canBuildPath)
        {
            if (placementSystem.IsBuildingStartNode()) return;

            Vector3 startPosition = placementSystem.StartPosition;
            Vector3 controlPosition;
            if (placementSystem.IsBuildingControlNode())
            {
                controlPosition = (startPosition + hitPosition) / 2;
            }
            else
            {
                controlPosition = placementSystem.ControlPosition;
            }
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
                placementSystem.BuildingState = SplinePlacementSystem.NodeBuildingState.ControlNode;
                return;
            }

            if (placementSystem.IsBuildingControlNode()) {
                placementSystem.ControlPosition = hitPosition;
                placementSystem.BuildingState = SplinePlacementSystem.NodeBuildingState.EndNode;
                return;
            }

            if (placementSystem.IsBuildingEndNode()) {
                placementSystem.EndPosition = hitPosition;
                placementSystem.PlacePath();
                //pathPlacementSystem.SplitPath();
                placementSystem.BuildingState = SplinePlacementSystem.NodeBuildingState.ControlNode;
                return;
            }
        }
        public void StopPreviewDisplay()
        {
            previewSystem.StopPreview();
        }
    }
}
