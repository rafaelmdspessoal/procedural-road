using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Path.Entities.SO;
using Global.UI;
using Path.PlacementSystem;

namespace Path.UI {
    public class PathUIController : MonoBehaviour {
        public static PathUIController Instance { get; private set; }

        private InputManager inputManager;
        private UIController uIController;
        private PathPlacementSystem pathPlacementManager;

        public Action<PathSO> OnBuildingStraightPath, OnBuildingCurvedPath, OnBuildingFreePath;
        public Action OnGridSnapping, OnAngleSnapping, OnPathUp, OnPathDown;

        [SerializeField] private Button straightPathButton;
        [SerializeField] private Button curvePathButton;
        [SerializeField] private Button freePathButton;


        [SerializeField] private Button gridSnapButton;
        [SerializeField] private Button angleSnapButton;
        [SerializeField] private Button pathUpButton;
        [SerializeField] private Button pathDownButton;

        [SerializeField] private TextMeshProUGUI angleSnapText;

        [SerializeField] private PathSO pathObjectSO;

        private void Awake() {
            Instance = this;

            DesablePathOptions();
        }

        private void Start() {
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;
            pathPlacementManager = PathPlacementSystem.Instance;

            inputManager.OnEscape += InputManager_OnEscape;
            uIController.OnBuildingPath += UIController_OnBuildingPath;
            uIController.OnRemovingObjects += UIController_OnRemovingObjects;
            pathPlacementManager.OnAngleSnapChanged += PathPlacementManager_OnAngleSnapChanged;

            // Snap desabled by default
            UpdateSnapAngleText(0);

            straightPathButton.onClick.AddListener(() => {
                OnBuildingStraightPath?.Invoke(pathObjectSO);
            });
            curvePathButton.onClick.AddListener(() => {
                OnBuildingCurvedPath?.Invoke(pathObjectSO);
            });
            freePathButton.onClick.AddListener(() => {
                OnBuildingFreePath?.Invoke(pathObjectSO);
            });

            gridSnapButton.onClick.AddListener(() => {
                OnGridSnapping?.Invoke();
            });
            angleSnapButton.onClick.AddListener(() => {
                OnAngleSnapping?.Invoke();
            });
            pathUpButton.onClick.AddListener(() => {
                OnPathUp?.Invoke();
            });
            pathDownButton.onClick.AddListener(() => {
                OnPathDown?.Invoke();
            });
        }

        private void PathPlacementManager_OnAngleSnapChanged(int angle) {
            UpdateSnapAngleText(angle);
        }

        private void UIController_OnRemovingObjects() {
            DesablePathOptions();
        }

        private void UIController_OnBuildingPath() {
            straightPathButton.interactable = true;
            curvePathButton.interactable = true;
            freePathButton.interactable = true;
            gridSnapButton.interactable = true;
            angleSnapButton.interactable = true;
            pathUpButton.interactable = true;
            pathDownButton.interactable = true;
        }

        private void InputManager_OnEscape() {
            DesablePathOptions();
        }

        private void DesablePathOptions() {
            straightPathButton.interactable = false;
            curvePathButton.interactable = false;
            freePathButton.interactable = false;
            gridSnapButton.interactable = false;
            angleSnapButton.interactable = false;
            pathUpButton.interactable = false;
            pathDownButton.interactable = false;
        }

        private void UpdateSnapAngleText(int angle) {
            angleSnapText.text = "Angle Snap: " + angle;
        }
    }
}