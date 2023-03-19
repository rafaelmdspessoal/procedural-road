using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Controller.Road {
    public class RoadUIController : MonoBehaviour {
        public static RoadUIController Instance { get; private set; }

        private InputManager inputManager;
        private UIController uIController;

        public Action<RoadObjectSO> OnBuildingRoad;

        [SerializeField] private Button straightRoadButton;
        [SerializeField] private Button curveRoadButton;
        [SerializeField] private Button freeRoadButton;

        [SerializeField] private RoadObjectSO roadObjectSO;

        private void Awake() {
            Instance = this;

            straightRoadButton.interactable = false;
            curveRoadButton.interactable = false;
            freeRoadButton.interactable = false;
        }

        private void Start() {
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;
            inputManager.OnEscape += InputManager_OnEscape;
            uIController.OnBuildingRoads += UIController_OnBuildingRoads;
            uIController.OnRemovingObjects += UIController_OnRemovingObjects;

            straightRoadButton.onClick.AddListener(() => {
                OnBuildingRoad?.Invoke(roadObjectSO);
            });
            curveRoadButton.onClick.AddListener(() => {
                OnBuildingRoad?.Invoke(roadObjectSO);
            });
            freeRoadButton.onClick.AddListener(() => {
                OnBuildingRoad?.Invoke(roadObjectSO);
            });
        }

        private void UIController_OnRemovingObjects() {
            DesableRoadOptions();
        }

        private void UIController_OnBuildingRoads() {
            straightRoadButton.interactable = true;
            curveRoadButton.interactable = true;
            freeRoadButton.interactable = true;
        }

        private void InputManager_OnEscape() {
            DesableRoadOptions();
        }

        private void DesableRoadOptions() {
            straightRoadButton.interactable = false;
            curveRoadButton.interactable = false;
            freeRoadButton.interactable = false;
        }
    }
}