using System.Collections.Generic;
using Normal.Realtime;
using UnityEngine;

namespace Normal.Utility {
    /// <summary>
    /// This component toggles other GameObjects/components on the avatar prefab,
    /// based on if it's a local avatar or a remote avatar.
    /// This is useful to ex hide the name tag on the local player.
    /// </summary>
    public class AvatarSetup : RealtimeComponent<AvatarSetupModel> {
        /// <summary>
        /// The GameObjects to enable on the local avatar (and disable on the remote avatars).
        /// </summary>
        [Header("Local Avatar")]
        [SerializeField]
        private List<GameObject> _localOnlyGameObjects;

        /// <summary>
        /// The components to enable on the local avatar (and disable on the remote avatars).
        /// </summary>
        [SerializeField]
        private List<MonoBehaviour> _localOnlyComponents;

        /// <summary>
        /// The GameObjects to enable on the remote avatars (and disable on the local avatar).
        /// </summary>
        [Header("Remote Avatar")]
        [SerializeField]
        private List<GameObject> _remoteOnlyGameObjects;

        /// <summary>
        /// The components to enable on the remote avatars (and disable on the local avatar).
        /// </summary>
        [SerializeField]
        private List<MonoBehaviour> _remoteOnlyComponents;

        protected override void OnRealtimeModelReplaced(AvatarSetupModel previousModel, AvatarSetupModel currentModel) {
            if (currentModel != null) {
                Setup(currentModel.isOwnedLocallyInHierarchy);
            }
        }

        private void Setup(bool isLocal) {
            // Iterate over lists and activate/deactivate

            var isRemote = isLocal == false;

            foreach (var go in _localOnlyGameObjects) {
                go.SetActive(isLocal);
            }

            foreach (var component in _localOnlyComponents) {
                component.enabled = isLocal;
            }

            foreach (var go in _remoteOnlyGameObjects) {
                go.SetActive(isRemote);
            }

            foreach (var component in _remoteOnlyComponents) {
                component.enabled = isRemote;
            }
        }
    }

    [RealtimeModel]
    public partial class AvatarSetupModel {

    }
}
