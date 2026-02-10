using System;
using System.Collections.Generic;
using Normal.Realtime;
using UnityEngine;

namespace Normal.GorillaTemplate.Cosmetics {
    /// <summary>
    /// The component that synchronizes the state of <see cref="Cosmetics"/> that are its children in the prefab hierarchy.
    /// </summary>
    [ExecuteAlways]
    public class CosmeticsSync : RealtimeComponent<CosmeticsSyncModel> {
        /// <summary>
        /// A wrapper around a <see cref="Cosmetic"/> reference used for the sake of <see cref="CosmeticEntryDrawer"/>.
        /// </summary>
        [Serializable]
        public struct CosmeticEntry {
            /// <summary>
            /// The referenced cosmetic.
            /// </summary>
            public Cosmetic cosmetic;
        }

        public IEnumerable<Cosmetic> cosmetics {
            get {
                foreach (var entry in _cosmetics) {
                    if (entry.cosmetic == null) {
                        continue;
                    }
                    yield return entry.cosmetic;
                }
            }
        }

        /// <summary>
        /// Cosmetics where <see cref="Cosmetic.hideFromLocalCamera"/> is true will be assigned
        /// this layer on the local client to hide it from the local camera.
        /// This is useful for ex sunglasses and hats that might interfere with the local camera.
        /// </summary>
        [SerializeField]
        private string _hideFromLocalCameraLayer;

        /// <summary>
        /// The list of <see cref="Cosmetic"/> instances managed by this component.
        /// </summary>
        [SerializeField]
        private List<CosmeticEntry> _cosmetics = new List<CosmeticEntry>();

        /// <summary>
        /// The enabled <see cref="Cosmetic"/> instances.
        /// </summary>
        private readonly HashSet<string> _enabledCosmetics = new HashSet<string>();

#if UNITY_EDITOR
        /// <summary>
        /// Refreshes the list of <see cref="Cosmetic"/> instances.
        /// </summary>
        public void RefreshCosmeticsList() {
            var cosmeticsInChildren = GetComponentsInChildren<Cosmetic>(true);

            _cosmetics.Clear();
            foreach (var cosmetic in cosmeticsInChildren) {
                _cosmetics.Add(new CosmeticEntry() {
                    cosmetic = cosmetic,
                });
            }
        }

        private void Reset() {
            RefreshCosmeticsList();
        }

        private void Awake() {
            UnityEditor.Undo.undoRedoPerformed += HandleUndoRedo;
        }

        private void OnDestroy() {
            UnityEditor.Undo.undoRedoPerformed -= HandleUndoRedo;
        }

        private void HandleUndoRedo() {
            if (TryGetIsOwnedRemotelyInHierarchy(out var ownedRemotely) && ownedRemotely) {
                return;
            }

            _enabledCosmetics.Clear();
            foreach (var entry in _cosmetics) {
                if (entry.cosmetic == null) {
                    continue;
                }

                if (entry.cosmetic.cosmeticEnabled) {
                    _enabledCosmetics.Add(entry.cosmetic.cosmeticName);
                }
            }
        }
#endif

        /// <summary>
        /// Resolves a cosmetic by name.
        /// </summary>
        public bool TryGetCosmeticByName(string cosmeticName, out Cosmetic cosmetic) {
            foreach (var entry in _cosmetics) {
                if (entry.cosmetic == null) {
                    continue;
                }
                if (entry.cosmetic.cosmeticName == cosmeticName) {
                    cosmetic = entry.cosmetic;
                    return true;
                }
            }

            cosmetic = default;
            return false;
        }

        /// <summary>
        /// Sets the state of the specified <see cref="Cosmetic"/> instance.
        /// </summary>
        /// <param name="value">Shown if true, hidden if false.</param>
        public void SetEnabled(Cosmetic cosmetic, bool value) {
            SetEnabled(cosmetic.cosmeticName, value);
        }

        /// <summary>
        /// Sets the state of the <see cref="Cosmetic"/> instance with the specified name.
        /// </summary>
        /// <param name="value">Shown if true, hidden if false.</param>
        public void SetEnabled(string cosmeticName, bool value) {
            if (!EnsureIsOwnedLocallyInHierarchy()) {
                return;
            }

            var cosmeticIsCurrentlyEnabled = _enabledCosmetics.Contains(cosmeticName);
            if (cosmeticIsCurrentlyEnabled == value) {
                return;
            }

            if (value) {
                _enabledCosmetics.Add(cosmeticName);
            } else {
                _enabledCosmetics.Remove(cosmeticName);
            }

            UpdateModel();

            // Set the state on the local cosmetic
            var localCosmeticFound = false;
            foreach (var entry in _cosmetics) {
                if (entry.cosmetic == null) {
                    continue;
                }

                if (entry.cosmetic.cosmeticName == cosmeticName) {
                    localCosmeticFound = true;
                    entry.cosmetic.cosmeticEnabled = value;
                    break;
                }
            }

            if (localCosmeticFound == false) {
                Debug.LogWarning($"No cosmetic found with the name '{cosmeticName}'");
            }
        }

        /// <summary>
        /// Disables (hides) all cosmetics.
        /// </summary>
        public void Clear() {
            if (!EnsureIsOwnedLocallyInHierarchy()) {
                return;
            }

            _enabledCosmetics.Clear();

            UpdateModel();

            // Disable all local cosmetics
            foreach (var entry in _cosmetics) {
                if (entry.cosmetic == null) {
                    continue;
                }

                entry.cosmetic.cosmeticEnabled = false;
            }
        }

#region Serialization

        /// <summary>
        /// The serialized state of this component.
        /// </summary>
        [Serializable]
        private class JsonData {
            /// <summary>
            /// A list of the enabled cosmetics' names.
            /// </summary>
            public List<string> enabledCosmetics = new List<string>();

            public void CopyFromHashSet(HashSet<string> source) {
                enabledCosmetics.Clear();
                foreach (var cosmeticName in source) {
                    enabledCosmetics.Add(cosmeticName);
                }
            }

            public void CopyToHashSet(HashSet<string> destination) {
                destination.Clear();
                foreach (var cosmeticName in enabledCosmetics) {
                    destination.Add(cosmeticName);
                }
            }
        }

        /// <summary>
        /// An instance that we re-use the prevent allocations.
        /// </summary>
        private readonly JsonData _tempData = new JsonData();

        /// <summary>
        /// Returns the serialized state as a JSON string.
        /// </summary>
        public string SaveJson() {
            _tempData.CopyFromHashSet(_enabledCosmetics);
            var json = JsonUtility.ToJson(_tempData, true);
            return json;
        }

        /// <summary>
        /// Loads a serialized JSON string.
        /// For internal use: doesn't update the model or the cosmetics.
        /// </summary>
        /// <returns>True if successfully loaded.</returns>
        private bool TryDeserializeJson(string json) {
            // Use a try-catch block in case the JSON string is invalid
            try {
                JsonUtility.FromJsonOverwrite(json, _tempData);
                _tempData.CopyToHashSet(_enabledCosmetics);
                return true;
            } catch (Exception e) {
                Debug.LogError($"Failed to parse cosmetics JSON data: {e}\n{json}");
                return false;
            }
        }

        /// <summary>
        /// Loads a serialized JSON string.
        /// </summary>
        /// <returns>True if successfully loaded.</returns>
        public bool TryLoadJson(string json) {
            if (!EnsureIsOwnedLocallyInHierarchy()) {
                return false;
            }

            if (TryDeserializeJson(json)) {
                UpdateModel();
                RefreshStateOfLocalCosmetics();
                return true;
            }

            return false;
        }

#endregion

#region Sync

        /// <summary>
        /// An instance that we re-use the prevent allocations.
        /// </summary>
        private static List<Renderer> _tempRenderers = new List<Renderer>();

        protected override void OnRealtimeModelReplaced(CosmeticsSyncModel previousModel, CosmeticsSyncModel currentModel) {
            if (previousModel != null) {
                previousModel.serializedDataDidChange -= OnSerializedDataChanged;
            }

            if (currentModel != null) {
                currentModel.serializedDataDidChange += OnSerializedDataChanged;

                // Hide the local player's cosmetics from the local camera
                if (currentModel.isOwnedLocallyInHierarchy && string.IsNullOrWhiteSpace(_hideFromLocalCameraLayer) == false) {
                    var hideFromLocalCameraLayer = LayerMask.NameToLayer(_hideFromLocalCameraLayer);
                    if (hideFromLocalCameraLayer == -1) {
                        Debug.LogError($"No layer found with name '{_hideFromLocalCameraLayer}'");
                    }

                    foreach (var entry in _cosmetics) {
                        if (entry.cosmetic == null) {
                            continue;
                        }

                        // Assign the layer to every GameObject on the cosmetic that has a Renderer component
                        if (entry.cosmetic.hideFromLocalCamera) {
                            entry.cosmetic.GetComponentsInChildren(true, _tempRenderers);
                            foreach (var cosmeticRenderer in _tempRenderers) {
                                cosmeticRenderer.gameObject.layer = hideFromLocalCameraLayer;
                            }
                        }
                    }
                }

                if (currentModel.isFreshModel) {
                    UpdateModel();
                } else {
                    // Deserialize and apply the data
                    if (TryDeserializeJson(currentModel.serializedData)) {
                        RefreshStateOfLocalCosmetics();
                    }
                }
            }
        }

        /// <summary>
        /// Fired when the model property changes.
        /// </summary>
        private void OnSerializedDataChanged(CosmeticsSyncModel currentModel, string value) {
            // Skip for the owning client (assume this data is already derived from local state - and the two are in sync)
            if (model.isOwnedLocallyInHierarchy) {
                return;
            }

            // Deserialize and apply the data
            if (TryDeserializeJson(value)) {
                RefreshStateOfLocalCosmetics();
            }
        }

        /// <summary>
        /// Refresh the state of the local cosmetic instances.
        /// </summary>
        private void RefreshStateOfLocalCosmetics() {
            foreach (var entry in _cosmetics) {
                if (entry.cosmetic == null) {
                    continue;
                }

                // A cosmetic should be enabled if it's inside the set of enabled cosmetics
                var shouldBeEnabled = _enabledCosmetics.Contains(entry.cosmetic.cosmeticName);

                // Set the state
                entry.cosmetic.cosmeticEnabled = shouldBeEnabled;
            }
        }

        /// <summary>
        /// Updates the model to reflect the state of the component.
        /// Should only be called on the owning client.
        /// </summary>
        private void UpdateModel() {
            if (model != null) {
                // Update the model property
                var json = SaveJson();
                model.serializedData = json;
            }
        }

        /// <summary>
        /// Checks ownership.
        /// </summary>
        /// <param name="value">Set to true if this component is owned remotely in hierarchy.</param>
        /// <returns>
        /// Returns if the operation succeeded.
        /// It can fail if ex there's no model on this component yet (we're not yet connected to a room).
        /// </returns>
        public bool TryGetIsOwnedRemotelyInHierarchy(out bool value) {
            if (model == null) {
                value = default;
                return false;
            }

            value = isOwnedRemotelyInHierarchy;
            return true;
        }

        private bool EnsureIsOwnedLocallyInHierarchy() {
            if (TryGetIsOwnedRemotelyInHierarchy(out var ownedRemotely) && ownedRemotely) {
                Debug.LogError($"This operation can only be invoked on the owning client, but is invoked on a remote client. Skipping.");
                return false;
            }

            return true;
        }

#endregion
    }

    /// <summary>
    /// The networked data of a <see cref="CosmeticsSync"/> component.
    /// </summary>
    [RealtimeModel]
    public partial class CosmeticsSyncModel {
        /// <summary>
        /// The list of the enabled cosmetics' names, serialized as JSON.
        /// </summary>
        [RealtimeProperty(1, true, true)]
        private string _serializedData;
    }
}
