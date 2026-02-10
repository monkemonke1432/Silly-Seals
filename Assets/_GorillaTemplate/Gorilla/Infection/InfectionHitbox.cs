using UnityEngine;

namespace Normal.GorillaTemplate.Infection {
    /// <summary>
    /// A hitbox that exists for remote players.
    /// When the local player touches a hitbox, the local player sends a request to tag the remote player.
    /// </summary>
    public class InfectionHitbox : MonoBehaviour {
        [SerializeField]
        private InfectionPlayerSync _infectionPlayerSync;

        /// <summary>
        /// The sync component of the player that this hitbox belongs to.
        /// </summary>
        public InfectionPlayerSync infectionPlayerSync {
            get => _infectionPlayerSync;
            set => _infectionPlayerSync = value;
        }

        private void OnTriggerEnter(Collider other) {
            if (_infectionPlayerSync != null) {
                // Assume we can only be triggered by the local player.
                // This is guaranteed by activating the infection interactors only on the local avatar.

                InfectionGameMode.instance.RequestToInfectOtherPlayer(_infectionPlayerSync);
            }
        }
    }
}
