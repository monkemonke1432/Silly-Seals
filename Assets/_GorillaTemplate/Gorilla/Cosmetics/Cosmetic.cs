using UnityEngine;

namespace Normal.GorillaTemplate.Cosmetics {
    /// <summary>
    /// A cosmetic that can be shown or hidden (synced over the network).
    /// It's shown or hidden by enabling or disabling its GameObject respectively.
    /// </summary>
    /// <remarks>
    /// It should be placed as a child of <see cref="CosmeticsSync"/> in the prefab hierarchy.
    /// </remarks>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class Cosmetic : MonoBehaviour {
        /// <summary>
        /// When true, this cosmetic is hidden from the local camera on the local client.
        /// Ex for sunglasses and hats that might interfere with the local camera.
        /// </summary>
        public bool hideFromLocalCamera => _hideFromLocalCamera;

        /// <inheritdoc cref="hideFromLocalCamera"/>
        [SerializeField]
        private bool _hideFromLocalCamera;

        /// <summary>
        /// The name of the cosmetic.
        /// This corresponds to the name of its <see cref="GameObject"/>.
        /// </summary>
        public virtual string cosmeticName => gameObject.name;

        /// <summary>
        /// The show/hide state of the cosmetic.
        /// </summary>
        public virtual bool cosmeticEnabled {
            get => isActiveAndEnabled;
            set => gameObject.SetActive(value);
        }

        /// <summary>
        /// Invoked when the GameObject is activated or the component is enabled.
        /// </summary>
        protected virtual void OnEnable() {
            NotifySyncComponent(true);
        }

        /// <summary>
        /// Invoked when the GameObject is de-activated or the component is disabled.
        /// </summary>
        protected virtual void OnDisable() {
            NotifySyncComponent(false);
        }

        /// <summary>
        /// Notifies the sync component about this cosmetic's new state.
        /// </summary>
        private void NotifySyncComponent(bool newState) {
#if UNITY_EDITOR
            if (UnityEditor.Undo.isProcessing) {
                return;
            }
#endif

            var sync = GetComponentInParent<CosmeticsSync>();

            if (sync == null) {
                return;
            }

            // Only proceed on the owning client
            if (sync.TryGetIsOwnedRemotelyInHierarchy(out var ownedRemotely) && ownedRemotely) {
                return;
            }

            // Notify the sync component
            sync.SetEnabled(this, newState);
        }
    }
}
