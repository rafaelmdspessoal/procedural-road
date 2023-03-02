using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private enum State {
        NotBuilding,
        Building,
    }

    private State state;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        state = State.NotBuilding;
        RoadUIController.Instance.OnStartPlacingRoads += RoadUIController_OnStartPlacingRoads;
    }

    private void RoadUIController_OnStartPlacingRoads() {
        state = State.Building;
    }

    public bool IsBuilding() {
        return state == State.Building;
    }
}
