using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Controller {
    public class UIController : MonoBehaviour {
        public static UIController Instance { get; private set; }
        public Action OnBuildingRoads;
        public Action OnRemovingObjects;

        [SerializeField] private Button buildRoadsButton;
        [SerializeField] private Button removeRoadsButton;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            buildRoadsButton.onClick.AddListener(() => {
                OnBuildingRoads?.Invoke();
            });
            removeRoadsButton.onClick.AddListener(() => {
                OnRemovingObjects?.Invoke();
            });
        }
    }    
}
