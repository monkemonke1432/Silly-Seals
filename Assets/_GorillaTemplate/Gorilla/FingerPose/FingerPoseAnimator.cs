using System;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Drives an animator given a <see cref="FingerPoseSync"/>.
    /// </summary>
    public class FingerPoseAnimator : MonoBehaviour {
        /// <summary>
        /// Makes fingers move smoothly instead of snapping to the latest hardware value.
        /// </summary>
        private struct FingerParameterSmoothing {
            public float current;
            public float target;
            private float _currentVelocity;

            /// <param name="smoothTime">How long (in seconds) it would take to reach the target value.</param>
            public void Smoothen(float smoothTime) {
                current = Mathf.SmoothDamp(current, target, ref _currentVelocity, smoothTime);
            }
        }

        /// <summary>
        /// The component that will drive the animation.
        /// </summary>
        [SerializeField]
        private FingerPoseSync _poseSync;

        /// <summary>
        /// The animator to play finger animations.
        /// </summary>
        [SerializeField]
        private Animator _animator;

        /// <summary>
        /// The time (in seconds) it takes a finger animation to reach the hardware pose.
        /// </summary>
        [SerializeField]
        [Range(0f, 0.5f)]
        private float _smoothTime = 0.1f;

        private FingerParameterSmoothing _indexParameter;
        private FingerParameterSmoothing _middleParameter;
        private FingerParameterSmoothing _thumbParameter;

        // These names correspond to normalized float parameters in the animator controller
        private static readonly int __indexClosedID = Animator.StringToHash("Index Closed");
        private static readonly int __middleClosedID = Animator.StringToHash("Middle Closed");
        private static readonly int __thumbClosedID = Animator.StringToHash("Thumb Closed");

        private void Awake() {
            if (_poseSync != null) {
                _poseSync.onFingerStateChanged += OnFingerStateChanged;
            }
        }

        private void OnDestroy() {
            if (_poseSync != null) {
                _poseSync.onFingerStateChanged -= OnFingerStateChanged;
                _poseSync = null;
            }
        }

        /// <summary>
        /// Read the hardware pose.
        /// </summary>
        private void OnFingerStateChanged(FingerType fingerType, float value) {
            if (_animator == null)
                return;

            switch (fingerType) {
                case FingerType.Index:
                    _indexParameter.target = value;
                    break;
                case FingerType.Middle:
                    _middleParameter.target = value;
                    break;
                case FingerType.Thumb:
                    _thumbParameter.target = value;
                    break;
                default:
                    throw new ArgumentException($"Unknown {nameof(FingerType)}: {fingerType}");
            }
        }

        /// <summary>
        /// Send the smoothened finger poses to the animator.
        /// </summary>
        private void Update() {
            _indexParameter.Smoothen(_smoothTime);
            _middleParameter.Smoothen(_smoothTime);
            _thumbParameter.Smoothen(_smoothTime);

            _animator.SetFloat(__indexClosedID, _indexParameter.current);
            _animator.SetFloat(__middleClosedID, _middleParameter.current);
            _animator.SetFloat(__thumbClosedID, _thumbParameter.current);
        }
    }
}
