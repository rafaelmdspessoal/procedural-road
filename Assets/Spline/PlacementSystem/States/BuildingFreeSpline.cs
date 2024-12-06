using UnityEngine;
using Rafael.Utils;
using Spline.Utils;

namespace Spline.Placement {

    public class BuildingFreeSpline: IPlacementState {

        private SplinePlacementSystem placementSystem;
        private SplinePreviewSystem previewSystem;

        public BuildingFreeSpline(SplinePlacementSystem placementSystem)
        {
            this.placementSystem = placementSystem;
            previewSystem = SplinePreviewSystem.Instance;
        }


        public void UpdateState(Vector3 hitPosition, bool canBuildPath)
        {
            if (placementSystem.IsBuildingStartNode()) return;

            Vector3 controlPosition;
            Vector3 startPosition = placementSystem.StartPosition;

            hitPosition = placementSystem.GetPositionForMinPathLengh(hitPosition);
            if (placementSystem.StartNode.HasConnection)
            {
                controlPosition = RafaelUtils.GetProjectedPosition(
                    hitPosition,
                    placementSystem.StartNode.direction,
                    startPosition);
            }
            else
            {
                controlPosition = (startPosition + hitPosition) / 2;
            }
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

            if (placementSystem.IsBuildingStartNode()) 
            {
                placementSystem.StartPosition = hitPosition;
                placementSystem.BuildingState = SplinePlacementSystem.NodeBuildingState.EndNode;
                return;
            }

            if (placementSystem.IsBuildingEndNode())
            {
                Vector3 startPosition = placementSystem.StartPosition;
                Vector3 controlPosition;
                if (placementSystem.StartNode.HasConnection)
                {
                    controlPosition = RafaelUtils.GetProjectedPosition(
                        hitPosition,
                        placementSystem.StartNode.direction,
                        startPosition);
                }
                else
                {
                    controlPosition = (startPosition + hitPosition) / 2;
                }
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
