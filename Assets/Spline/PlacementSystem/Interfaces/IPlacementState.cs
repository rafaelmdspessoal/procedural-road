using Spline.Placement;
using UnityEngine;

public interface IPlacementState
{
    public void UpdateState(Vector3 hitPosition, bool canBuildPath);
    public void OnAction(Vector3 hitPosition, bool canBuildPath);

    public void StopPreviewDisplay();
}