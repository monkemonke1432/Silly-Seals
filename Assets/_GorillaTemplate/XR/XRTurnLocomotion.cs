using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Normal.XR {
    /// <summary>
    /// Rotates a transform in response to user input.
    /// Supports smooth and snap turn modes.
    /// </summary>
    public class XRTurnLocomotion : MonoBehaviour {
        public enum TurnMode {
            /// <summary>
            /// Continuous turn.
            /// </summary>
            Smooth,

            /// <summary>
            /// Discrete intervals, which can help with motion sickness.
            /// </summary>
            Snap,

            /// <summary>
            /// Turning is disabled.
            /// </summary>
            Disabled,
        }

        /// <summary>
        /// The turn mode to use.
        /// </summary>
        [Header("Options")]
        public TurnMode turnMode;

        /// <summary>
        /// For <see cref="TurnMode.Smooth"/>, the turn speed in degrees/second.
        /// </summary>
        public float smoothTurnSpeed = 45f;

        /// <summary>
        /// For <see cref="TurnMode.Snap"/>, the turn increment of every snap turn.
        /// </summary>
        public float snapTurnIncrement = 30f;

        /// <summary>
        /// The InputSystem stick action for smooth turn.
        /// XR controllers don't have X/Y axis controls, so we use stick and ignore the Y value.
        /// </summary>
        [Header("Input Actions")]
        public InputActionReference smoothTurnDirection;

        /// <summary>
        /// The InputSystem stick action for snap turn.
        /// The sign of the X value is used to determine the turn direction, but the X value itself is ignored.
        /// XR controllers don't have X/Y axis controls, so we use stick and ignore the Y value.
        /// </summary>
        public InputActionReference snapTurnDirection;

        /// <summary>
        /// The player to turn.
        /// </summary>
        [Header("Output")]
        public GorillaLocomotion.Player player;

        protected void Update() {
            Step(Time.deltaTime);
        }

        private void Step(float deltaTime) {
            if (turnMode == TurnMode.Smooth) {
                // We use a Vector2 although we only use the X component.
                // This is because there's currently no Axis/composite controls on XR joysticks:
                // https://forum.unity.com/threads/how-to-map-thumbstick-axis-to-a-button-action.824559/.
                var inputDirection = smoothTurnDirection.action.ReadValue<Vector2>().x;
                var directionDelta = inputDirection * (smoothTurnSpeed * deltaTime);

                player.Turn(directionDelta, false);
            } else if (turnMode == TurnMode.Snap) {
                if (snapTurnDirection.action.WasPerformedThisFrame()) {
                    var inputDirection = snapTurnDirection.action.ReadValue<Vector2>().x;

                    // Determine direction by sign
                    if (!Mathf.Approximately(0f, inputDirection))
                        inputDirection = inputDirection > 0f ? 1f : -1f;

                    var directionDelta = inputDirection * snapTurnIncrement;

                    player.Turn(directionDelta, true);
                }
            } else if (turnMode == TurnMode.Disabled) {
                // Do nothing
            } else {
                throw new Exception($"Unexpected {nameof(TurnMode)}: {turnMode}");
            }
        }
    }
}
