using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Represents a character that has two hands that can grab objects.
    /// </summary>
    public class Grabber : MonoBehaviour {
        [SerializeField]
        private GrabberHand _rightHand;

        [SerializeField]
        private GrabberHand _leftHand;

        /// <summary>
        /// This Transform is used to convert device tracking data
        /// (ex linear or angular XR controller velocity reported by the Unity Input System) into world-space coordinates.
        /// </summary>
        /// <seealso cref="GrabberHand._handLinearVelocityAction"/>
        /// <seealso cref="GrabberHand._handAngularVelocityAction"/>
        [SerializeField]
        private Transform _bodyRoot;

        [SerializeField]
        private ThrowSettings _throwSettings = ThrowSettings.defaultValue;

        public GrabberHand rightHand { get => _rightHand; set => _rightHand = value; }

        public GrabberHand leftHand { get => _leftHand; set => _leftHand = value; }

        /// <inheritdoc cref="_bodyRoot"/>
        public Transform bodyRoot { get => _bodyRoot; set => _bodyRoot = value; }

        public ThrowSettings throwSettings { get => _throwSettings; set => _throwSettings = value; }

        /// <summary>
        /// Releases the objects held in the left and right hands (if any).
        /// </summary>
        public void ReleaseObjects() {
            _rightHand.grabbedObject = null;
            _leftHand.grabbedObject = null;
        }
    }
}
