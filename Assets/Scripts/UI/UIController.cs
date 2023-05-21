using System;
using UnityEngine;
using UnityEngine.UI;

namespace Global.UI {
    public class UIController : MonoBehaviour {
        public static UIController Instance { get; private set; }
        public Action OnBuildingObjects;
        public Action OnRemovingObjects;

        private InputManager inputManager;

        [SerializeField] private Transform selectObjectTypeToBuildTransform;

        [SerializeField] private Button buildObjectsButton;
        [SerializeField] private Button removeObjectsButton;

        private void Awake() {
            Instance = this;
        }

        private void Start()
        {
            selectObjectTypeToBuildTransform.gameObject.SetActive(false);
            buildObjectsButton.gameObject.SetActive(true);

            inputManager = InputManager.Instance;
            inputManager.OnEscape += InputManager_OnEscape;

            buildObjectsButton.onClick.AddListener(BuildObjectsButton_onClick);
            removeObjectsButton.onClick.AddListener(RemoveObjectsButton_onClick);
        }

        private void InputManager_OnEscape()
        {
            selectObjectTypeToBuildTransform.gameObject.SetActive(false);
            buildObjectsButton.gameObject.SetActive(true);
        }

        private void BuildObjectsButton_onClick()
        {
            OnBuildingObjects?.Invoke();
            selectObjectTypeToBuildTransform.gameObject.SetActive(true);
            buildObjectsButton.gameObject.SetActive(false);
        }
        private void RemoveObjectsButton_onClick()
        {
            OnRemovingObjects?.Invoke();
            selectObjectTypeToBuildTransform.gameObject.SetActive(false);
            buildObjectsButton.gameObject.SetActive(true);
        }
    }    
}
