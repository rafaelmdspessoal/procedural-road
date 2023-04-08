using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roads.Placement;
using UI.Controller;

namespace UI.Roads.Controller{
    public class RoadUIController : MonoBehaviour {
        public static RoadUIController Instance { get; private set; }

        private InputManager inputManager;
        private UIController uIController;
        private RoadPlacementSystem roadPlacementManager;

        public Action<RoadObjectSO> OnBuildingStraightRoad, OnBuildingCurvedRoad, OnBuildingFreeRoad;
        public Action OnGridSnapping, OnAngleSnapping, OnRoadUp, OnRoadDown;

        [SerializeField] private Button straightRoadButton;
        [SerializeField] private Button curveRoadButton;
        [SerializeField] private Button freeRoadButton;


        [SerializeField] private Button gridSnapButton;
        [SerializeField] private Button angleSnapButton;
        [SerializeField] private Button roadUpButton;
        [SerializeField] private Button roadDownButton;

        [SerializeField] private TextMeshProUGUI angleSnapText;

        [SerializeField] private RoadObjectSO roadObjectSO;

        private void Awake() {
            Instance = this;

            DesableRoadOptions();
        }

        private void Start() {
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;
            roadPlacementManager = RoadPlacementSystem.Instance;

            inputManager.OnEscape += InputManager_OnEscape;
            uIController.OnBuildingRoads += UIController_OnBuildingRoads;
            uIController.OnRemovingObjects += UIController_OnRemovingObjects;
            roadPlacementManager.OnAngleSnapChanged += RoadPlacementManager_OnAngleSnapChanged;

            // Snap desabled by default
            UpdateSnapAngleText(0);

            straightRoadButton.onClick.AddListener(() => {
                OnBuildingStraightRoad?.Invoke(roadObjectSO);
            });
            curveRoadButton.onClick.AddListener(() => {
                OnBuildingCurvedRoad?.Invoke(roadObjectSO);
            });
            freeRoadButton.onClick.AddListener(() => {
                OnBuildingFreeRoad?.Invoke(roadObjectSO);
            });

            gridSnapButton.onClick.AddListener(() => {
                OnGridSnapping?.Invoke();
            });
            angleSnapButton.onClick.AddListener(() => {
                OnAngleSnapping?.Invoke();
            });
            roadUpButton.onClick.AddListener(() => {
                OnRoadUp?.Invoke();
            });
            roadDownButton.onClick.AddListener(() => {
                OnRoadDown?.Invoke();
            });
        }

        private void RoadPlacementManager_OnAngleSnapChanged(int angle) {
            UpdateSnapAngleText(angle);
        }

        private void UIController_OnRemovingObjects() {
            DesableRoadOptions();
        }

        private void UIController_OnBuildingRoads() {
            straightRoadButton.interactable = true;
            curveRoadButton.interactable = true;
            freeRoadButton.interactable = true;
            gridSnapButton.interactable = true;
            angleSnapButton.interactable = true;
            roadUpButton.interactable = true;
            roadDownButton.interactable = true;
        }

        private void InputManager_OnEscape() {
            DesableRoadOptions();
        }

        private void DesableRoadOptions() {
            straightRoadButton.interactable = false;
            curveRoadButton.interactable = false;
            freeRoadButton.interactable = false;
            gridSnapButton.interactable = false;
            angleSnapButton.interactable = false;
            roadUpButton.interactable = false;
            roadDownButton.interactable = false;
        }

        private void UpdateSnapAngleText(int angle) {
            angleSnapText.text = "Angle Snap: " + angle;
        }
    }
}