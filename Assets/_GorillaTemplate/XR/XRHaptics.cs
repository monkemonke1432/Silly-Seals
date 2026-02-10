using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.XR.Haptics;

namespace Normal.XR {
    /// <summary>
    /// Plays haptic effects on an XR controller.
    /// </summary>
    public class XRHaptics : MonoBehaviour {
        /// <summary>
        /// Specify the left or right controller.
        /// </summary>
        public enum Hand {
            Left,
            Right,
        }

        /// <summary>
        /// Specify which controller.
        /// </summary>
        [SerializeField]
        private Hand _hand;

        /// <summary>
        /// The default amplitude of the haptic impulse.
        /// </summary>
        [SerializeField]
        private float _amplitude = 0.5f;

        /// <summary>
        /// The default duration of the haptic impulse.
        /// </summary>
        [SerializeField]
        private float _duration = 0.1f;

        /// <summary>
        /// Resolves the hardware device.
        /// </summary>
        private static XRController GetDevice(Hand hand) {
            switch (hand) {
                case Hand.Left:
                    return XRController.leftHand;
                case Hand.Right:
                    return XRController.rightHand;
                default:
                    throw new Exception($"Unknown {nameof(Hand)}: {hand}");
            }
        }

        /// <summary>
        /// Play a haptic impulse with the specified amplitude and duration.
        /// </summary>
        public void Rumble(float amplitudeOverride, float durationOverride) {
            var device = GetDevice(_hand);

            if (device != null) {
                var command = SendHapticImpulseCommand.Create(0, amplitudeOverride, durationOverride);
                device.ExecuteCommand(ref command);
            }
        }

        /// <summary>
        /// Play a haptic impulse with the specified amplitude and the default duration.
        /// </summary>
        public void Rumble(float amplitudeOverride) {
            Rumble(amplitudeOverride, _duration);
        }

        /// <summary>
        /// Play a haptic impulse with the default amplitude and duration.
        /// </summary>
        public void Rumble() {
            Rumble(_amplitude, _duration);
        }
    }
}
