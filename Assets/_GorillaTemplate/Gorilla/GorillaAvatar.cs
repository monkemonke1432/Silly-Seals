using Normal.Realtime;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// This component holds a Gorilla player's components.
    /// Exists on a <see cref="Normal.Realtime.RealtimeAvatar"/> instance.
    /// </summary>
    public class GorillaAvatar : MonoBehaviour {
        [SerializeField]
        private RealtimeAvatarVoice _voice;

        [SerializeField]
        private PlayerDataSync _playerDataSync;

        /// <summary>
        /// A layer that's excluded from the local camera. Used to hide parts of the local player.
        /// </summary>
        [SerializeField]
        private string _localCullingLayer;

        /// <summary>
        /// The head mesh to hide on the local player (so that it doesn't show up on the camera).
        /// </summary>
        [SerializeField]
        private GameObject _headMesh;

        /// <summary>
        /// A component that syncs player data like the player name.
        /// </summary>
        public PlayerDataSync playerDataSync => _playerDataSync;

        /// <summary>
        /// A component that implements voice chat.
        /// </summary>
        public RealtimeAvatarVoice voice => _voice;

        private int _localCullingLayerCached;

        private void Awake() {
            _localCullingLayerCached = LayerMask.NameToLayer(_localCullingLayer);
            if (_localCullingLayerCached == -1) {
                Debug.LogError($"No layer found with name '{_localCullingLayer}'");
            }
        }

        /// <summary>
        /// Hides the head from the camera. Useful for the local player.
        /// </summary>
        public void HideHeadMesh() {
            _headMesh.layer = _localCullingLayerCached;
        }
    }
}
