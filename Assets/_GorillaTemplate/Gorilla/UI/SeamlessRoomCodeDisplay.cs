using Normal.SeamlessRooms;
using UnityEngine;

namespace Normal.GorillaTemplate.UI {
    /// <summary>
    /// Displays the current room name for a <see cref="SeamlessRoomConnecter"/>.
    /// </summary>
    public class SeamlessRoomCodeDisplay : MonoBehaviour {
        /// <summary>
        /// The text field that will hold the name.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Text _visual;

        [SerializeField]
        private SeamlessRoomConnecter _connecter;

        private void Update() {

            string description;

            if (_connecter.roomInProgress == null) {
                description = DisplayRealtimeStatus();
            } else {
                // A connection to a different room is in progress
                description = DisplayRoomInProgressStatus();
            }

            _visual.text = $"Current room: {description}";
        }

        private string DisplayRealtimeStatus() {
            var realtime = _connecter.realtime;

            // Check connection state
            if (realtime.disconnected) {
                return "Not connected";
            } else if (realtime.connecting) {
                return "Connecting...";
            } else if (realtime.connected) {
                // Use the room name
                return realtime.room.name;
            } else {
                return null;
            }
        }

        private string DisplayRoomInProgressStatus() {
            return "Connecting...";
        }
    }
}
