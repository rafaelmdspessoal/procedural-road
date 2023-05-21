using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Path.Entities.SO;
using Global.UI;
using Path.PlacementSystem;
using System.Collections.Generic;
using Path.Entities.Vehicle.SO;
using Path.Entities.Pedestrian.SO;
using Path.Entities.Pedestrian;

namespace Path.UI {
    public class PathUIController : MonoBehaviour {
        public static PathUIController Instance { get; private set; }

        private InputManager inputManager;
        private UIController uIController;
        private PathPlacementSystem pathPlacementManager;

        public Action OnBuildingStraightPath, OnBuildingCurvedPath, OnBuildingFreePath;
        public Action OnGridSnapping, OnAngleSnapping, OnPathUp, OnPathDown;
        public Action<GameObject> OnObjectSelected;

        [SerializeField] private Transform objectsToBuild;
        [SerializeField] private Transform objectsToBuildPannel;
        [SerializeField] private Transform pathOptionsPannel;

        [SerializeField] private GameObject buildObjectButtonPrefab;

        [SerializeField] private Button buildPathButton;
        [SerializeField] private Button buildRoadButton;

        [SerializeField] private List<VehiclePathSO> vehiclePathList;
        [SerializeField] private List<PedestrianPathSO> pedestrianPathList;

        [SerializeField] private Button straightPathButton;
        [SerializeField] private Button curvePathButton;
        [SerializeField] private Button freePathButton;

        [SerializeField] private Button gridSnapButton;
        [SerializeField] private Button angleSnapButton;
        [SerializeField] private Button pathUpButton;
        [SerializeField] private Button pathDownButton;

        [SerializeField] private TextMeshProUGUI angleSnapText;

        private void OnEnable()
        {
            DesablePathOptions();
            buildRoadButton.gameObject.SetActive(true);
            buildPathButton.gameObject.SetActive(true);
        }

        private void Awake() {
            Instance = this;
            ClearChildren();
        }

        private void Start() {
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;
            pathPlacementManager = PathPlacementSystem.Instance;

            inputManager.OnEscape += InputManager_OnEscape;
            uIController.OnBuildingObjects += UIController_OnBuildingObjects;
            uIController.OnRemovingObjects += UIController_OnRemovingObjects;
            pathPlacementManager.OnAngleSnapChanged += PathPlacementManager_OnAngleSnapChanged;

            DesablePathOptions();
            buildPathButton.onClick.AddListener(BuildPathButton_onClick);
            buildRoadButton.onClick.AddListener(BuildRoadButton_onClick);

            // Snap desabled by default
            UpdateSnapAngleText(0);

            straightPathButton.onClick.AddListener(() => { OnBuildingStraightPath?.Invoke(); });
            curvePathButton.onClick.AddListener(() => {OnBuildingCurvedPath?.Invoke();});
            freePathButton.onClick.AddListener(() => {OnBuildingFreePath?.Invoke();});

            gridSnapButton.onClick.AddListener(() => {OnGridSnapping?.Invoke();});
            angleSnapButton.onClick.AddListener(() => {OnAngleSnapping?.Invoke();});

            pathUpButton.onClick.AddListener(() => {OnPathUp?.Invoke();});
            pathDownButton.onClick.AddListener(() => { OnPathDown?.Invoke(); });
        }

        private void BuildPathButton_onClick()
        {
            ClearChildren();
            buildRoadButton.gameObject.SetActive(false);
            pathOptionsPannel.gameObject.SetActive(true);
            objectsToBuildPannel.gameObject.SetActive(true);
            foreach (PedestrianPathSO pedestrianPath in pedestrianPathList)
            {
                BuildObjectButton btn = Instantiate(buildObjectButtonPrefab, objectsToBuild).GetComponent<BuildObjectButton>();
                btn.objectToBuild = pedestrianPath.pathObjectPrefab;
                btn.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    BuildObjectButton_onClick(btn.objectToBuild);
                    objectsToBuildPannel.gameObject.SetActive(false);
                });
            }
        }
        private void BuildRoadButton_onClick()
        {
            ClearChildren();
            buildPathButton.gameObject.SetActive(false);
            pathOptionsPannel.gameObject.SetActive(true);
            objectsToBuildPannel.gameObject.SetActive(true);
            foreach (VehiclePathSO vehiclePath in vehiclePathList)
            {
                BuildObjectButton btn = Instantiate(buildObjectButtonPrefab, objectsToBuild).GetComponent<BuildObjectButton>();
                btn.objectToBuild = vehiclePath.pathObjectPrefab;
                btn.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    BuildObjectButton_onClick(btn.objectToBuild);
                    objectsToBuildPannel.gameObject.SetActive(false);
                });
            }
        }

        private void BuildObjectButton_onClick(GameObject obj)
        {
            OnObjectSelected?.Invoke(obj);
        }

        private void PathPlacementManager_OnAngleSnapChanged(int angle) {
            UpdateSnapAngleText(angle);
        }

        private void UIController_OnRemovingObjects() {
            DesablePathOptions();
            ClearChildren();
        }

        private void UIController_OnBuildingObjects() 
        {
            pathOptionsPannel.gameObject.SetActive(true);
        }

        private void InputManager_OnEscape() {
            DesablePathOptions();
            ClearChildren();
        }

        private void DesablePathOptions() 
        {
            objectsToBuildPannel.gameObject.SetActive(false);
            pathOptionsPannel.gameObject.SetActive(false);
        }

        private void UpdateSnapAngleText(int angle) {
            angleSnapText.text = "Angle Snap: " + angle;
        }

        private void ClearChildren()
        {
            foreach (Transform child in objectsToBuild)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
