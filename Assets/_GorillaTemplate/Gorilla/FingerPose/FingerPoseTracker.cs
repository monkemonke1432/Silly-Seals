using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Uses InputSystem actions to get finger poses from the hardware device.
    /// </summary>
    public class FingerPoseTracker : MonoBehaviour {
        /// <summary>
        /// A float value action for the index finger (0 = open, 1 = closed).
        /// </summary>
        [SerializeField]
        private InputActionReference _indexFingerClosed;

        /// <summary>
        /// A float value action for the middle finger (0 = open, 1 = closed).
        /// </summary>
        [SerializeField]
        private InputActionReference _middleFingerClosed;

        /// <summary>
        /// A float value action for the thumb finger (0 = open, 1 = closed).
        /// </summary>
        [SerializeField]
        private InputActionReference _thumbFingerClosed;

        private float _indexFingerValue;
        private float _middleFingerValue;
        private float _thumbFingerValue;

        /// <summary>
        /// Invoked when the state of a finger changes.
        /// </summary>
        public event Action<FingerType, float> onFingerStateChanged;

        /// <summary>
        /// Returns the most recent state of the specified finger.
        /// </summary>
        public float GetFingerState(FingerType type) {
            switch (type) {
                case FingerType.Index:
                    return _indexFingerValue;
                case FingerType.Middle:
                    return _middleFingerValue;
                case FingerType.Thumb:
                    return _thumbFingerValue;
                default:
                    throw new ArgumentException($"Unexpected {nameof(FingerType)}: {type}");
            }
        }

        private void OnEnable() {
            _indexFingerClosed.action.Enable();
            _middleFingerClosed.action.Enable();
            _thumbFingerClosed.action.Enable();

            // These are value actions, so we want to listen to both performed and canceled
            _indexFingerClosed.action.performed += OnIndexFingerPerformed;
            _indexFingerClosed.action.canceled += OnIndexFingerCanceled;
            _middleFingerClosed.action.performed += OnMiddleFingerPerformed;
            _middleFingerClosed.action.canceled += OnMiddleFingerCanceled;
            _thumbFingerClosed.action.performed += OnThumbFingerPerformed;
            _thumbFingerClosed.action.canceled += OnThumbFingerCanceled;
        }

        private void OnIndexFingerPerformed(InputAction.CallbackContext ctx) {
            _indexFingerValue = ctx.ReadValue<float>();
            onFingerStateChanged?.Invoke(FingerType.Index, _indexFingerValue);
        }

        private void OnIndexFingerCanceled(InputAction.CallbackContext ctx) {
            _indexFingerValue = ctx.ReadValue<float>();
            onFingerStateChanged?.Invoke(FingerType.Index, _indexFingerValue);
        }

        private void OnMiddleFingerPerformed(InputAction.CallbackContext ctx) {
            _middleFingerValue = ctx.ReadValue<float>();
            onFingerStateChanged?.Invoke(FingerType.Middle, _middleFingerValue);
        }

        private void OnMiddleFingerCanceled(InputAction.CallbackContext ctx) {
            _middleFingerValue = ctx.ReadValue<float>();
            onFingerStateChanged?.Invoke(FingerType.Middle, _middleFingerValue);
        }

        private void OnThumbFingerPerformed(InputAction.CallbackContext ctx) {
            _thumbFingerValue = ctx.ReadValue<float>();
            onFingerStateChanged?.Invoke(FingerType.Thumb, _thumbFingerValue);
        }

        private void OnThumbFingerCanceled(InputAction.CallbackContext ctx) {
            _thumbFingerValue = ctx.ReadValue<float>();
            onFingerStateChanged?.Invoke(FingerType.Thumb, _thumbFingerValue);
        }
    }
}
