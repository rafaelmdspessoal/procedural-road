using System;
using UnityEngine;
using UnityEngine.UI;

namespace Global.UI {
    public class UIController : MonoBehaviour {
        public static UIController Instance { get; private set; }
        public Action OnBuildingPath;
        public Action OnRemovingObjects;

        [SerializeField] private Button buildPathButton;
        [SerializeField] private Button removePathButton;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            buildPathButton.onClick.AddListener(() => {
                OnBuildingPath?.Invoke();
            });
            removePathButton.onClick.AddListener(() => {
                OnRemovingObjects?.Invoke();
            });
        }
    }    
}
