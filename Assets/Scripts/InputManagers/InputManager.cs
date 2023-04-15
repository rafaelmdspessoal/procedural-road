using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UI.Controller;
using Rafael.Utils;
using Roads.Utilities;

public class InputManager : MonoBehaviour
{
	public static InputManager Instance { get; private set; }

	public Action OnEscape;
	public Action OnCancel;

	public event EventHandler<OnObjectHitedEventArgs> OnSelected;
	public event EventHandler<OnObjectHitedEventArgs> OnNodePlaced;
	public event EventHandler<OnObjectHitedEventArgs> OnObjectRemoved;
	public class OnObjectHitedEventArgs : EventArgs {
		public Vector3 position;
		public GameObject obj;
	}

    private GameInputActions gameInputActions;

	private UIController UIController;

	private void Awake() {
		Instance = this;
		gameInputActions = new();

		gameInputActions.Idle.Enable();
		gameInputActions.Idle.Select.performed += Select_performed;

		gameInputActions.General.Enable();
        gameInputActions.General.Escape.performed += Building_Escape_performed;

		gameInputActions.Destroying.Demolish.performed += Demolish_performed;

        gameInputActions.BuildingRoad.PlaceNode.performed += PlaceNode_performed;
        gameInputActions.BuildingRoad.Cancel.performed += Cancel_performed;
	}

    private void Start() {
		UIController = UIController.Instance;
		UIController.OnBuildingRoads += RoadUIController_OnBuildingRoads;
		UIController.OnRemovingObjects += RoadUIController_OnRemovingObjects;

	}

    private void RoadUIController_OnRemovingObjects() {
		gameInputActions.Idle.Disable();
		gameInputActions.Destroying.Enable();
		gameInputActions.BuildingRoad.Disable(); ;
    }

    private void RoadUIController_OnBuildingRoads() {
		gameInputActions.Idle.Disable();
		gameInputActions.Destroying.Disable();
		gameInputActions.BuildingRoad.Enable();
    }

    private void Cancel_performed(InputAction.CallbackContext obj) {
		OnCancel?.Invoke();
    }

    private void PlaceNode_performed(InputAction.CallbackContext obj) {
		if (EventSystem.current.IsPointerOverGameObject()) return;

		if (RoadUtilities.TryRaycastObject(out Vector3 hitPosition, out GameObject hitObject, splitRoad: true)) {
            OnNodePlaced?.Invoke(this, new OnObjectHitedEventArgs {
				position = hitPosition,
				obj = hitObject
			});
		}
	}

    private void Building_Escape_performed(InputAction.CallbackContext obj) {
		ResetToIdle();
		OnEscape?.Invoke();
	}

    private void Demolish_performed(InputAction.CallbackContext obj) {
		if (EventSystem.current.IsPointerOverGameObject()) return;

		if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
			OnObjectRemoved?.Invoke(this, new OnObjectHitedEventArgs {
				position = hit.point,
				obj = hit.transform.gameObject
			});
		}

	}

	private void Select_performed(InputAction.CallbackContext obj) {
		if (EventSystem.current.IsPointerOverGameObject()) return;

		if (RafaelUtils.TryRaycastObject(out RaycastHit hit)) {
			OnSelected?.Invoke(this, new OnObjectHitedEventArgs {
				position = hit.point,
				obj = hit.transform.gameObject
			});
		}
	}
		
	private void ResetToIdle() {
		gameInputActions.BuildingRoad.Disable();
		gameInputActions.Destroying.Disable();
		gameInputActions.Idle.Enable();
	}
}

