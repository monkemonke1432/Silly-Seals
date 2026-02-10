using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace Normal.XR {
    /// <summary>
    /// A component that, when a hand controller has lost tracking, pulls it towards a pre-defined pose.
    /// This prevents the player's hand from getting frozen in awkward poses.
    /// </summary>
    public class XRIdleHandPose : MonoBehaviour {
        /// <summary>
        /// The InputSystem action that provides the controller's tracking state.
        /// </summary>
        [SerializeField]
        private InputActionReference _trackingStateAction;

        /// <summary>
        /// The pre-defined pose that the hand will be pulled towards.
        /// </summary>
        [SerializeField]
        private Transform _idleTargetTransform;

        /// <summary>
        /// The hand's transform. This transform will be pulled.
        /// </summary>
        [SerializeField]
        private Transform _handTransform;

        /// <summary>
        /// (Optional) The InputSystem TrackedPoseDriver that is driving the hand.
        /// </summary>
        [SerializeField]
        private TrackedPoseDriver _trackedPoseDriver;

        /// <summary>
        /// Controls the smoothness/responsiveness of the pull.
        /// </summary>
        [SerializeField]
        private float _smoothingFactor = 0.2f;

        private InputTrackingState _currentTrackingState;

        private void OnEnable() {
            _trackingStateAction.action.performed += OnTrackingStatePerformed;
            _trackingStateAction.action.canceled += OnTrackingStateCanceled;

            // Poll the initial state
            _currentTrackingState = (InputTrackingState)_trackingStateAction.action.ReadValue<int>();
        }

        private void OnDisable() {
            _trackingStateAction.action.performed -= OnTrackingStatePerformed;
            _trackingStateAction.action.canceled -= OnTrackingStateCanceled;
        }

        private void OnTrackingStatePerformed(InputAction.CallbackContext context) {
            _currentTrackingState = (InputTrackingState)context.ReadValue<int>();
        }

        private void OnTrackingStateCanceled(InputAction.CallbackContext context) {
            _currentTrackingState = InputTrackingState.None;
        }

        private void LateUpdate() {
            var hasPositionTracking = _currentTrackingState.HasFlag(InputTrackingState.Position);
            var hasRotationTracking = _currentTrackingState.HasFlag(InputTrackingState.Rotation);

            if (_trackedPoseDriver != null) {
                // The TrackedPoseDriver doesn't always read the initial tracking state properly,
                // thinking tracking is present and tries to drive the transform.
                //
                // This conflicts with our pull, so we enable/disable the driver based on our
                // knowledge of the controller's tracking state instead.
                _trackedPoseDriver.enabled = hasPositionTracking && hasRotationTracking;
            }

            if (!hasPositionTracking || !hasRotationTracking) {
                _handTransform.position = Vector3.Lerp(_handTransform.position, _idleTargetTransform.position, _smoothingFactor);
                _handTransform.rotation = Quaternion.Slerp(_handTransform.rotation, _idleTargetTransform.rotation, _smoothingFactor);
            }
        }
    }
}
