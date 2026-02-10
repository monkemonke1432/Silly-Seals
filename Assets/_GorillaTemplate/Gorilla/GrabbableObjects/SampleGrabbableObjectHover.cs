using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Demonstrates how to display a hover effect on a <see cref="GrabbableObject"/>.
    /// </summary>
    [RequireComponent(typeof(GrabbableObject))]
    public class SampleGrabbableObjectHover: MonoBehaviour {
        /// <summary>
        /// The color to display when a hand starts hovering over the object.
        /// </summary>
        [SerializeField]
        private Color _hoverColor = Color.white;

        private GrabbableObject _grabbableObject;
        private Renderer _renderer;
        private Color _originalColor;

        private void Awake() {
            _grabbableObject = GetComponent<GrabbableObject>();
            _renderer = _grabbableObject.GetComponentInChildren<Renderer>();
            _originalColor = _renderer.material.color;
        }

        private void LateUpdate() {
            _renderer.material.color = _grabbableObject.isHovered ? _hoverColor : _originalColor;
        }
    }
}
