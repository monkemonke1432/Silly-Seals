using System.Collections.Generic;
using UnityEngine;

namespace Normal.GorillaTemplate.Infection {
    /// <summary>
    /// Holds a list of hitboxes, ex all hitboxes belonging to a player prefab.
    /// Makes it simpler to operate on them as a group.
    /// </summary>
    public class InfectionHitboxGroup : MonoBehaviour {
        [SerializeField]
        private List<InfectionHitbox> _hitboxes;

        public IReadOnlyList<InfectionHitbox> hitboxes => _hitboxes;

        /// <summary>
        /// Enables or disables all hitboxes in the group.
        /// </summary>
        public void SetHitboxesEnabled(bool isEnabled) {
            foreach (var hitbox in _hitboxes) {
                hitbox.enabled = isEnabled;
            }
        }

        /// <summary>
        /// Assigns a sync component to each hitbox in the group.
        /// </summary>
        public void SetSync(InfectionPlayerSync infectionPlayerSync) {
            foreach (var hitbox in _hitboxes) {
                hitbox.infectionPlayerSync = infectionPlayerSync;
            }
        }
    }
}
