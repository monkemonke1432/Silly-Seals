using System;
using Normal.Realtime;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Syncs player data across the network.
    /// </summary>
    public class PlayerDataSync : RealtimeComponent<PlayerDataSyncModel> {
        [SerializeField]
        private string _nameTag = "Name Tag";

        [SerializeField]
        private Color _color = Color.white;

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string nameTag {
            get => _nameTag;
            set {
                if (_nameTag == value) {
                    return;
                }

                _nameTag = value;
                NotifyChangedLocally();
            }
        }

        /// <summary>
        /// The color of the player.
        /// </summary>
        public Color color {
            get => _color;
            set {
                if (_color == value) {
                    return;
                }

                _color = value;
                NotifyChangedLocally();
            }
        }

        /// <summary>
        /// Called by <see cref="Editor.PlayerDataSyncEditor"/> to reflect editor changes across the network.
        /// </summary>
        internal void NotifyChangedLocally() {
            if (model != null) {
                model.nameTag = _nameTag;
                model.color = _color;
            } else {
                onChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Dispatched when the player data changes locally or remotely.
        /// </summary>
        public event Action<PlayerDataSync> onChanged;

        [SerializeField]
        private TMPro.TMP_Text _nameTagVisual;

        /// <summary>
        /// The text element that displays the name.
        /// </summary>
        public TMPro.TMP_Text nameTagVisual {
            get => _nameTagVisual;
            set {
                if (_nameTagVisual == value) {
                    return;
                }

                _nameTagVisual = value;
                SyncVisuals();
            }
        }

        [SerializeField]
        private GorillaSkin _skin;

        /// <summary>
        /// The component that controls the player skin.
        /// </summary>
        public GorillaSkin skin {
            get => _skin;
            set {
                if (_skin == value) {
                    return;
                }

                _skin = value;
                SyncVisuals();
            }
        }

        /// <summary>
        /// Updates the visual representation using the current value.
        /// </summary>
        private void SyncVisuals() {
            if (model == null) {
                return;
            }

            if (_nameTagVisual != null) {
                _nameTagVisual.text = string.IsNullOrEmpty(model.nameTag) ? "No name" : model.nameTag;
            }

            if (_skin != null) {
                _skin.baseColor = model.color;
            }
        }

        protected override void OnRealtimeModelReplaced(PlayerDataSyncModel previousModel, PlayerDataSyncModel currentModel) {
            if (previousModel != null) {
                previousModel.nameTagDidChange -= OnNameTagDidChange;
                previousModel.colorDidChange   -= OnColorDidChange;
            }

            if (currentModel != null) {
                currentModel.nameTagDidChange += OnNameTagDidChange;
                currentModel.colorDidChange   += OnColorDidChange;

                if (model.isFreshModel) {
                    currentModel.nameTag = _nameTag;
                    currentModel.color   = _color;
                } else {
                    _nameTag = currentModel.nameTag;
                    _color   = currentModel.color;

                    SyncVisuals();
                }
            }
        }

        private void OnNameTagDidChange(PlayerDataSyncModel playerDataSyncModel, string value) {
            _nameTag = value;
            SyncVisuals();
            onChanged?.Invoke(this);
        }

        private void OnColorDidChange(PlayerDataSyncModel playerDataSyncModel, Color value) {
            _color = value;
            SyncVisuals();
            onChanged?.Invoke(this);
        }
    }

    [RealtimeModel]
    public partial class PlayerDataSyncModel {
        [RealtimeProperty(1, true, true)]
        private string _nameTag;

        [RealtimeProperty(2, true, true)]
        private Color _color;
    }
}
