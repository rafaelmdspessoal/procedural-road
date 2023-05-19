using UnityEngine;
using Path;
using Global.UI;

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
        uIController.OnBuildingPath += UIController_OnBuildingPath;
        uIController.OnRemovingObjects += UIController_OnRemovingObjects;

        inputManager.OnEscape += InputManager_OnEscape;
        inputManager.OnObjectRemoved += InputManager_OnObjectRemoved;
    }

    // NOTE: This should be moved to a more appropriate place
    private void InputManager_OnObjectRemoved(object sender, InputManager.OnObjectHitedEventArgs e) {
        if (IsDemolishing()) {
            if (e.obj.TryGetComponent(out IPath removeableObject)) {
                removeableObject.RemovePath();
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

    private void UIController_OnBuildingPath() {
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
