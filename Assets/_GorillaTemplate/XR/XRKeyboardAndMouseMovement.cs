using UnityEngine;
using UnityEngine.InputSystem;

namespace Normal.XR {
    /// <summary>
    /// Provides basic character movement using the keyboard and mouse.
    /// For debugging/testing purposes only.
    /// </summary>
    public class XRKeyboardAndMouseMovement : MonoBehaviour {
        [SerializeField]
        private Rigidbody _target;

        [SerializeField]
        private float _movementSpeed = 3f;

        [SerializeField]
        private float _jumpVelocity = 4f;

        [SerializeField]
        private float _turnSpeed = 35f;

        [SerializeField]
        private bool _holdLeftMouseToTurn = true;

        private void Update() {
            var targetTransform = _target.transform;

            PollMouse(out var deltaX);

            if (Mathf.Approximately(deltaX, 0f) == false) {
                // Turn around the Y axis
                var eulerDelta = new Vector3(0f, deltaX, 0f) * (_turnSpeed * Time.deltaTime);
                targetTransform.eulerAngles += eulerDelta;
            }

            PollKeyboard(out var localMovementDirection, out var jump);

            if (localMovementDirection != Vector3.zero) {
                // Adjust for current facing direction
                var globalMovementDirection = targetTransform.TransformDirection(localMovementDirection);

                // Move
                targetTransform.position += globalMovementDirection * (_movementSpeed * Time.deltaTime);
            }

            if (jump) {
                // Jump by setting the Rigidbody's velocity
                // Gravity is applied automatically by the Rigidbody
                _target.linearVelocity += Vector3.up * _jumpVelocity;
            }
        }

        private void PollKeyboard(out Vector3 movementDirection, out bool jump) {
            movementDirection = Vector3.zero;
            jump = false;

            var keyboard = Keyboard.current;
            if (keyboard == null) {
                return;
            }

            // Check WASD keys
            if (keyboard.wKey.isPressed) {
                movementDirection.z += 1f;
            }
            if (keyboard.sKey.isPressed) {
                movementDirection.z -= 1f;
            }
            if (keyboard.dKey.isPressed) {
                movementDirection.x += 1f;
            }
            if (keyboard.aKey.isPressed) {
                movementDirection.x -= 1f;
            }

            if (movementDirection != Vector3.zero) {
                movementDirection.Normalize();
            }

            // Check space bar
            jump = keyboard.spaceKey.wasPressedThisFrame;
        }

        private void PollMouse(out float deltaX) {
            deltaX = 0f;

            var mouse = Mouse.current;
            if (mouse == null) {
                return;
            }

            if (_holdLeftMouseToTurn && mouse.leftButton.isPressed == false) {
                return;
            }

            deltaX = mouse.delta.x.value;
        }
    }
}
