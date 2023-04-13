using UnityEngine;

public interface IBuildingState
{
    public void UpdateState(Vector3 hitPosition, RoadObjectSO roadObjectSO, bool canBuildRoad);
    public void OnAction(Vector3 hitPosition, bool canBuildRoad);
    public void StopPreviewDisplay();
}