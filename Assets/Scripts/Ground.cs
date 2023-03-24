using UnityEngine;

namespace World {

    public class Ground : MonoBehaviour {
        private void Start() {
            InputManager.Instance.OnSelected += InputManager_OnSelected;
            InputManager.Instance.OnDemolished += InputManager_OnDemolished;
        }

        private void InputManager_OnSelected(object sender, InputManager.OnObjectHitedEventArgs e) {
            // Debug.Log("Selected: " + e.obj.transform.name + " at: " + e.position);
        }

        private void InputManager_OnDemolished(object sender, InputManager.OnObjectHitedEventArgs e) {
            // Debug.Log("Destroyed: " + e.obj.transform.name + " at: " + e.position);
        }
    }
}
