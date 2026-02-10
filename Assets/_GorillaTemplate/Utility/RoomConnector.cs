using UnityEngine;

namespace Normal.Utility {
    /// <summary>
    /// Exposes a ConnectToRoom function with a single room name argument.
    /// Targets the <see cref="Realtime.Realtime"/> component that is on the same GameObject.
    /// This function can be assigned to a <see cref="UnityEngine.Events.UnityEvent"/> in the editor.
    /// </summary>
    [RequireComponent(typeof(Realtime.Realtime))]
    public class RoomConnector : MonoBehaviour {
        private Realtime.Realtime _realtime;

        private void Awake() {
            _realtime = GetComponent<Realtime.Realtime>();
        }

        public void ConnectToRoom(string roomName) {
            if (_realtime.disconnected == false && roomName == _realtime.room.name) {
                Debug.Log($"Already connecting or connected to {roomName}, ignoring the {nameof(ConnectToRoom)} to call");
                return;
            }

            // Disconnect if necessary
            if (_realtime.connected) {
                _realtime.Disconnect();
            }

            _realtime.Connect(roomName);
        }
    }
}
