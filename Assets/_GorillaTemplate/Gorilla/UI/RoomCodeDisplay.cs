using UnityEngine;

namespace Normal.GorillaTemplate.UI {
    /// <summary>
    /// Displays the current room name.
    /// </summary>
    public class RoomCodeDisplay : MonoBehaviour {
        /// <summary>
        /// The text field that will hold the name.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Text _visual;

        [SerializeField]
        private Realtime.Realtime _realtime;

        private void Update() {
            string description;

            // Check connection state
            if (_realtime.disconnected) {
                description = "Not connected";
            } else if (_realtime.connecting) {
                description = "Connecting...";
            } else if (_realtime.connected) {
                // Use the room name
                description = _realtime.room.name;
            } else {
                description = null;
            }

            _visual.text = $"Current room: {description}";
        }
    }
}
