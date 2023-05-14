using Path.Entities.SO;
using UnityEngine;

public interface IBuildingState
{
    public void UpdateState(Vector3 hitPosition, PathSO pathObjectSO, bool canBuildPath);
    public void OnAction(Vector3 hitPosition, bool canBuildPath);
    public void StopPreviewDisplay();
}