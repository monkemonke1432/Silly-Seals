using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Demonstrates how to use the events on a <see cref="GrabbableObject"/>.
    /// It can also be used for debugging events and to see how they behave over the network.
    /// </summary>
    [RequireComponent(typeof(GrabbableObject))]
    public class SampleGrabbableObjectEvents : MonoBehaviour {
        /// <summary>
        /// The color to display when a hand starts hovering over the object.
        /// </summary>
        [SerializeField]
        private Color _hoverEnterEffectColor = Color.white;

        /// <summary>
        /// The color to display when a hand stops hovering over the object.
        /// </summary>
        [SerializeField]
        private Color _hoverExitEffectColor = Color.black;

        [SerializeField]
        private float _hoverEffectDuration = 0.5f;

        /// <summary>
        /// The color to display when a hand grabs the object (set by a local client).
        /// </summary>
        [SerializeField]
        private Color _localGrabReleaseEffectColor = Color.green;

        /// <summary>
        /// The color to display when a hand grabs the object (set by a remote client).
        /// </summary>
        [SerializeField]
        private Color _remoteGrabReleaseEffectColor = Color.red;

        [SerializeField]
        private float _grabReleaseEffectDuration = 0.5f;

        private Renderer _renderer;
        private Color _originalColor;

        private float _hoverEnterEffectTimestamp;
        private float _hoverExitEffectTimestamp;
        private float _grabReleaseEffectTimestamp;
        private bool _grabReleaseIsLocal;

        private void Awake() {
            var grabbableObject = GetComponent<GrabbableObject>();
            _renderer = grabbableObject.GetComponentInChildren<Renderer>();
            _originalColor = _renderer.material.color;

            // Subscribe to hover events
            grabbableObject.onHoverEnter += OnHoverEnter;
            grabbableObject.onHoverExit += OnHoverExit;

            // Subscribe to grab events
            grabbableObject.onGrab += OnGrab;
            grabbableObject.onRelease += OnRelease;
        }

        private void OnHoverEnter(GrabberHand hand) {
            _hoverEnterEffectTimestamp = Time.time;
        }

        private void OnHoverExit(GrabberHand hand) {
            _hoverExitEffectTimestamp = Time.time;
        }

        private void OnGrab(GrabberHand hand) {
            _grabReleaseEffectTimestamp = Time.time;
            _grabReleaseIsLocal = hand.isOwnedLocallyInHierarchy;
        }

        private void OnRelease(GrabberHand hand) {
            _grabReleaseEffectTimestamp = Time.time;
            _grabReleaseIsLocal = hand.isOwnedLocallyInHierarchy;
        }

        private void LateUpdate() {
            // Check which effects are active
            var hoverEnterEffectIsActive = _hoverEnterEffectTimestamp > 0f && (Time.time - _hoverEnterEffectTimestamp) <= _hoverEffectDuration;
            var hoverExitEffectIsActive = _hoverExitEffectTimestamp > 0f && (Time.time - _hoverExitEffectTimestamp) <= _hoverEffectDuration;
            var grabReleaseEffectIsActive = _grabReleaseEffectTimestamp > 0f && (Time.time - _grabReleaseEffectTimestamp) <= _grabReleaseEffectDuration;

            // Display the most relevant effect
            if (grabReleaseEffectIsActive) {
                _renderer.material.color = _grabReleaseIsLocal ? _localGrabReleaseEffectColor : _remoteGrabReleaseEffectColor;
            } else if (hoverEnterEffectIsActive) {
                _renderer.material.color = _hoverEnterEffectColor;
            } else if (hoverExitEffectIsActive) {
                _renderer.material.color = _hoverExitEffectColor;
            } else {
                _renderer.material.color = _originalColor;
            }
        }
    }
}
