using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Global.UI;
using Path.Entities.Pedestrian.SO;
using System.Collections.Generic;

namespace Spline.UI {
    public class SplineUIController : MonoBehaviour {
        public static SplineUIController Instance { get; private set; }

        private InputManager inputManager;
        private UIController uIController;

        public Action OnStraightModeSelected, OnCurvedModeSelected, OnFreeModeSelected;
        public Action OnGridSnapping, OnAngleSnapping, OnPathUp, OnPathDown;
        //public Action<GameObject> OnObjectToBuildSelected;
        public Action OnObjectToBuildSelected;

        [SerializeField] private Transform objectsToBuild;
        [SerializeField] private Transform objectsToBuildPannel;
        [SerializeField] private Transform optionsPannel;

        [SerializeField] private GameObject buildObjectButtonPrefab;
        [SerializeField] private Button buildButton;
        [SerializeField] private List<GameObject> objectsToBuildList;

        [SerializeField] private Button straightButton;
        [SerializeField] private Button curveButton;
        [SerializeField] private Button freeButton;

        [SerializeField] private Button gridSnapButton;
        [SerializeField] private Button angleSnapButton;
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;

        [SerializeField] private TextMeshProUGUI angleSnapText;

        private void OnEnable()
        {
            DesableOptions();
            buildButton.gameObject.SetActive(true);
        }

        private void Awake() {
            Instance = this;
            ClearChildren();
        }

        private void Start() {
            inputManager = InputManager.Instance;
            uIController = UIController.Instance;

            inputManager.OnEscape += InputManager_OnEscape;
            uIController.OnBuildingObjects += UIController_OnBuildingObjects;
            uIController.OnRemovingObjects += UIController_OnRemovingObjects;

            DesableOptions();
            buildButton.onClick.AddListener(BuildSplineButton_OnClick);

            // Snap desabled by default
            UpdateSnapAngleText(0);

            straightButton.onClick.AddListener(() => { OnStraightModeSelected?.Invoke(); });
            curveButton.onClick.AddListener(() => {OnCurvedModeSelected?.Invoke();});
            freeButton.onClick.AddListener(() => {OnFreeModeSelected?.Invoke();});

            gridSnapButton.onClick.AddListener(() => {OnGridSnapping?.Invoke();});
            angleSnapButton.onClick.AddListener(() => {OnAngleSnapping?.Invoke();});

            upButton.onClick.AddListener(() => {OnPathUp?.Invoke();});
            downButton.onClick.AddListener(() => { OnPathDown?.Invoke(); });
        }

        private void BuildSplineButton_OnClick()
        {
            ClearChildren();
            optionsPannel.gameObject.SetActive(true);
            objectsToBuildPannel.gameObject.SetActive(true);
            foreach (GameObject objectsToBuild in objectsToBuildList)
            {
                GameObject btn = Instantiate(buildObjectButtonPrefab, this.objectsToBuild);
                // btn.objectToBuild = pedestrianPath.pathObjectPrefab;
                btn.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectObjectToBuild_OnClick();
                    // objectsToBuildPannel.gameObject.SetActive(false);
                });
            }
        }

        private void SelectObjectToBuild_OnClick()
        {
            OnObjectToBuildSelected?.Invoke();
        }

        private void PathPlacementManager_OnAngleSnapChanged(int angle) {
            UpdateSnapAngleText(angle);
        }

        private void UIController_OnRemovingObjects() {
            DesableOptions();
            ClearChildren();
        }

        private void UIController_OnBuildingObjects() 
        {
            optionsPannel.gameObject.SetActive(true);
        }

        private void InputManager_OnEscape() {
            DesableOptions();
            ClearChildren();
        }

        private void DesableOptions() 
        {
            objectsToBuildPannel.gameObject.SetActive(false);
            optionsPannel.gameObject.SetActive(false);
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
