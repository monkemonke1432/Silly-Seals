using Normal.Realtime;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// A component that resets the transform position and rotation on disconnect.
    /// Use this ex on scene grabbable objects to reset them when changing rooms.
    /// </summary>
    [RequireComponent(typeof(RealtimeView))]
    public class ResetTransformOnDisconnect : MonoBehaviour {
        private Realtime.Realtime _realtime;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        private void Awake() {
            var t = transform;
            _initialPosition = t.position;
            _initialRotation = t.rotation;
        }

        private void Start() {
            var view = GetComponent<RealtimeView>();
            _realtime = view.realtime;

            _realtime.didDisconnectFromRoom += OnDisconnect;
        }

        private void OnDestroy() {
            if (_realtime != null) {
                _realtime.didDisconnectFromRoom -= OnDisconnect;
            }
        }

        private void OnDisconnect(Realtime.Realtime realtime) {
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);

            // If there is a RealtimeTransform and Rigidbody, it won't budge unless we also update the Rigidbody like this:
            if (TryGetComponent(out Rigidbody rb)) {
                rb.position = _initialPosition;
                rb.rotation = _initialRotation;
            }
        }
    }
}
