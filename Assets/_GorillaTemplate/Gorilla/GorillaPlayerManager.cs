using System;
using System.Collections.Generic;
using Normal.GorillaTemplate.PlayerData;
using Normal.Realtime;
using Normal.XR;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Configures <see cref="GorillaAvatar"/> instances when they're spawned.
    /// </summary>
    [RequireComponent(typeof(RealtimeAvatarManager))]
    public class GorillaPlayerManager : MonoBehaviour {
        private readonly Dictionary<int, GorillaAvatar> _avatars = new Dictionary<int, GorillaAvatar>();

        public IReadOnlyDictionary<int, GorillaAvatar> avatars => _avatars;

        private RealtimeAvatarManager _avatarManager;
        private GorillaAvatar _localAvatar;

        public delegate void PlayerJoinedLeft(GorillaAvatar avatar, bool isLocalPlayer);
        public event PlayerJoinedLeft playerJoined;
        public event PlayerJoinedLeft playerLeft;

        private void Awake() {
            _avatarManager = GetComponent<RealtimeAvatarManager>();

            _avatarManager.avatarCreated   += OnAvatarCreated;
            _avatarManager.avatarDestroyed += OnAvatarDestroyed;
            LocalPlayerData.onChanged      += OnLocalPlayerDataChanged;
        }

        private void OnDestroy() {
            _avatarManager.avatarCreated   -= OnAvatarCreated;
            _avatarManager.avatarDestroyed -= OnAvatarDestroyed;
            LocalPlayerData.onChanged      -= OnLocalPlayerDataChanged;
        }

        private void OnAvatarCreated(RealtimeAvatarManager manager, RealtimeAvatar avatar, bool isLocalAvatar) {
            var gorillaAvatar = avatar.GetComponent<GorillaAvatar>();

            _avatars.Add(avatar.ownerIDInHierarchy, gorillaAvatar);

            if (isLocalAvatar) {
                _localAvatar = gorillaAvatar;

                OnLocalPlayerDataChanged();

                // Hide the head mesh (to prevent it from interfering with the camera)
                gorillaAvatar.HideHeadMesh();
            }

            playerJoined?.Invoke(gorillaAvatar, isLocalAvatar);
        }

        private void OnAvatarDestroyed(RealtimeAvatarManager manager, RealtimeAvatar avatar, bool isLocalAvatar) {
            if (_localAvatar) {
                _localAvatar = null;
            }

            if (_avatars.Remove(avatar.ownerIDInHierarchy, out var gorillaAvatar)) {
                playerLeft?.Invoke(gorillaAvatar, isLocalAvatar);
            }
        }

        private void OnLocalPlayerDataChanged() {
            // Setup local rig
            GorillaLocalRig.instance.turnMode = LocalPlayerData.turnMode switch {
                LocalPlayerData.TurnMode.Snap     => XRTurnLocomotion.TurnMode.Snap,
                LocalPlayerData.TurnMode.Smooth   => XRTurnLocomotion.TurnMode.Smooth,
                LocalPlayerData.TurnMode.Disabled => XRTurnLocomotion.TurnMode.Disabled,
                _ => throw new Exception($"Unexpected turn mode: {LocalPlayerData.turnMode}"),
            };
            GorillaLocalRig.instance.snapTurnIncrement = LocalPlayerData.snapTurnIncrement;
            GorillaLocalRig.instance.smoothTurnSpeed   = LocalPlayerData.smoothTurnSpeed;

            // Setup local avatar
            _localAvatar.playerDataSync.nameTag = LocalPlayerData.playerName;
            _localAvatar.playerDataSync.color   = LocalPlayerData.playerColor;
        }
    }
}
