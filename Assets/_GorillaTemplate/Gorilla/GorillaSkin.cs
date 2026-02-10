using System;
using System.Collections.Generic;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Swaps the texture on a player model.
    /// </summary>
    public class GorillaSkin : MonoBehaviour {
        /// <summary>
        /// Represents a saved material state that can be saved and restored at a later point.
        /// </summary>
        [Serializable]
        public struct MaterialState {
            public static readonly MaterialState defaultState = new MaterialState() {
                MainTexture = null,
                Color = Color.white,
            };

            /// <summary>
            /// The base texture.
            /// </summary>
            public Texture MainTexture;

            /// <summary>
            /// The base texture color multiplier.
            /// </summary>
            public Color Color;

            /// <summary>
            /// Creates an instance from a material.
            /// </summary>
            public MaterialState(Material material) {
                MainTexture = material.mainTexture;
                Color = material.color;
            }

            /// <summary>
            /// Applies the saved data onto a material.
            /// </summary>
            public void ApplyOnMaterial(Material material) {
                material.mainTexture = MainTexture;
                material.color = Color;
            }
        }

        /// <summary>
        /// The color that is applied on the renderers when <see cref="useSkinOverride"/> is false.
        /// </summary>
        public Color baseColor = Color.white;

        /// <summary>
        /// <see cref="skinOverride"/>
        /// </summary>
        public bool useSkinOverride;

        /// <summary>
        /// The material state to apply while <see cref="useSkinOverride"/> is true.
        /// </summary>
        public MaterialState skinOverride = MaterialState.defaultState;

        /// <summary>
        /// The renderers whose materials should be updated.
        /// </summary>
        [SerializeField]
        private List<Renderer> _targetRenderers = new List<Renderer>();

        /// <summary>
        /// The original states of the materials.
        /// These are restored when the player loses their infected status.
        /// </summary>
        private readonly List<MaterialState> _originalStates = new List<MaterialState>();

        private void Awake() {
            foreach (var r in _targetRenderers) {
                var originalState = new MaterialState(r.material);
                _originalStates.Add(originalState);
            }
        }

        private void Update() {
            var idx = 0;

            foreach (var r in _targetRenderers) {
                var originalState = _originalStates[idx];

                MaterialState newState;
                if (useSkinOverride) {
                    newState = skinOverride;
                } else {
                    newState = originalState;
                    newState.Color = baseColor;
                }

                newState.ApplyOnMaterial(r.material);
                idx++;
            }
        }
    }
}
