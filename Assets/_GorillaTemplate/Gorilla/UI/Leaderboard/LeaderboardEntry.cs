using Normal.GorillaTemplate.Keyboard;
using UnityEngine;
using UnityEngine.UI;

namespace Normal.GorillaTemplate.UI.Leaderboard {
    /// <summary>
    /// Represents a player inside a <see cref="Leaderboard"/>.
    /// </summary>
    public class LeaderboardEntry : MonoBehaviour {
        /// <summary>
        /// The color indicator of the player.
        /// </summary>
        [SerializeField]
        private Image _colorIndicator;

        /// <summary>
        /// The name of the player.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Text _nameLabel;

        /// <summary>
        /// A toggle to mute the player.
        /// </summary>
        [Header("Mute Toggle Button")]
        [SerializeField]
        private SimpleButton _muteToggleButton;

        [SerializeField]
        private Renderer _muteToggleButtonRenderer;

        [SerializeField]
        private Color _muteToggleInactiveColor = Color.green;

        [SerializeField]
        private Color _muteToggleActiveColor = Color.red;

        [SerializeField]
        private Renderer _muteToggleIconRenderer;

        [SerializeField]
        private Texture2D _muteToggleInactiveIcon;

        [SerializeField]
        private Texture2D _muteToggleActiveIcon;

        private GorillaAvatar _avatar;
        private bool          _isLocalPlayer;

        public void Initialize(GorillaAvatar avatar, bool isLocalPlayer) {
            // Save values
            _avatar = avatar;
            _isLocalPlayer = isLocalPlayer;

            _avatar.playerDataSync.onChanged += OnPlayerDataChanged;
            OnPlayerDataChanged(_avatar.playerDataSync);

            SetToggle(avatar.voice.mute);
            _muteToggleButton.onPressed.AddListener(OnMuteToggled);

            // Underline the name for the local player
            _nameLabel.fontStyle = isLocalPlayer ? TMPro.FontStyles.Underline : TMPro.FontStyles.Normal;
        }

        private void OnDestroy() {
            if (_avatar != null && _avatar.playerDataSync != null) {
                _avatar.playerDataSync.onChanged -= OnPlayerDataChanged;
            }

            if (_muteToggleButton != null) {
                _muteToggleButton.onPressed.RemoveListener(OnMuteToggled);
            }
        }

        private void OnPlayerDataChanged(PlayerDataSync playerDataSync) {
            _colorIndicator.color = playerDataSync.color;
            _nameLabel.text = string.IsNullOrEmpty(playerDataSync.nameTag) ? "No name" : playerDataSync.nameTag;
        }

        private void OnMuteToggled() {
            var shouldMute = !_avatar.voice.mute;
            _avatar.voice.mute = shouldMute;
            SetToggle(shouldMute);
        }

        private void SetToggle(bool muted) {
            _muteToggleButtonRenderer.material.color = muted ? _muteToggleActiveColor : _muteToggleInactiveColor;
            _muteToggleIconRenderer.material.mainTexture = muted ? _muteToggleActiveIcon : _muteToggleInactiveIcon;
        }
    }
}
