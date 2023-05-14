using UnityEngine;

namespace Path.Entities.Meshes
{
    public class MeshEdje : MonoBehaviour
    {
        public enum EdjePosition
        {
            StartCenter,
            StartLeft,
            StartRight,
            EndCenter,
            EndLeft,
            EndRight,
        }

        private EdjePosition edjePosition;

        public EdjePosition EdjePos => edjePosition;

        public void Init(EdjePosition edjePosition)
        {
            this.edjePosition = edjePosition;
            transform.name = edjePosition.ToString();
        }

        public Vector3 Position => transform.position;
        public Vector3 Direction => transform.forward;
    }
}
