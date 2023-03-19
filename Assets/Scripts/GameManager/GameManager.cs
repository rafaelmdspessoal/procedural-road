using UnityEngine;
using UI.Controller;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private InputManager inputManager;
    private UIController uIController;


    private enum State {
        Idle,
        Building,
        Demolishing,
    }

    private State state;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        inputManager = InputManager.Instance;
        uIController = UIController.Instance;

        state = State.Idle;
        uIController.OnBuildingRoads += RoadUIController_OnBuildingRoads;
        uIController.OnRemovingObjects += UIController_OnRemovingObjects;

        inputManager.OnEscape += InputManager_OnEscape;
        inputManager.OnDemolished += InputManager_OnDemolished;
    }

    private void InputManager_OnDemolished(object sender, InputManager.OnObjectHitedEventArgs e) {
        if (IsDemolishing()) {
            if (e.obj.TryGetComponent(out IRemoveable removeableObject)) {
                Debug.Log("object to remove: " + e.obj);
                removeableObject.Remove();
            }
        }
    }

    private void UIController_OnRemovingObjects() {
        state = State.Demolishing;
        Debug.Log("Game State: " + state);
    }

    private void InputManager_OnEscape() {
        state = State.Idle;
        Debug.Log("State: " + state);
    }

    private void RoadUIController_OnBuildingRoads() {
        state = State.Building;
        Debug.Log("State: " + state);
    }

    public bool IsBuilding() {
        return state == State.Building;
    }

    public bool IsDemolishing() {
        return state == State.Demolishing;
    }
}
