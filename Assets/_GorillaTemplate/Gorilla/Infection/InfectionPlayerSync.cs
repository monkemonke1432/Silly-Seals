using Normal.Utility;
using UnityEngine;

namespace Normal.GorillaTemplate.Infection {
    /// <summary>
    /// Polls the infection status of the associated player from the gamemode networked data.
    /// Also syncs infection requests over the network.
    /// The gamemode owner is the one who processes the requests and updates the gamemode state though.
    /// </summary>
    public class InfectionPlayerSync : MessageQueue<InfectionRequest> {
        /// <summary>
        /// The player skin to apply while infected.
        /// </summary>
        [SerializeField]
        private GorillaSkin.MaterialState _infectedSkin = GorillaSkin.MaterialState.defaultState;

        [SerializeField]
        private GorillaSkin _skin;

        /// <summary>
        /// The skin component to control based on the polled infection status.
        /// </summary>
        public GorillaSkin skin {
            get => _skin;
            set {
                if (_skin == value) {
                    return;
                }

                _skin = value;
                SyncSkinToModel();
            }
        }

        /// <summary>
        /// The last polled infection state of this player as reported by <see cref="InfectionGameMode"/>.
        /// </summary>
        private bool _infected;

        private void Update() {
            if (model == null) {
                return;
            }

            // Check if this player's infection state has changed
            var newInfected = InfectionGameMode.instance.IsInfected(ownerIDInHierarchy);
            if (newInfected != _infected) {
                _infected = newInfected;
                SyncSkinToModel();

                if (_infected && isOwnedLocallyInHierarchy) {
                    // Players get stunned when they get infected
                    GorillaLocalRig.instance.Stun(5f);
                }
            }
        }

        /// <summary>
        /// Syncs visuals with the player's infection state.
        /// </summary>
        private void SyncSkinToModel() {
            if (model == null || _skin == null) {
                return;
            }

            _skin.useSkinOverride = _infected;
            _skin.skinOverride = _infectedSkin;
        }

        /// <summary>
        /// Dispatches a request to infect the specified player.
        /// </summary>
        public void InfectOtherPlayer(int targetClientID) {
            if (model == null) {
                return;
            }

            var newEntry = new InfectionRequest() {
                targetClientID = targetClientID,
            };

            Send(newEntry);
        }

        /// <inheritdoc/>
        protected override void Process(InfectionRequest message) {
            // Only the confirmed owner of the gamemode will validate and apply this request
            InfectionGameMode.instance.TryInfectPlayer(ownerIDInHierarchy, message.targetClientID);
        }
    }

    [RealtimeModel]
    public partial class InfectionRequest {
        /// <summary>
        /// A remote client that the local player claims to have infected.
        /// </summary>
        [RealtimeProperty(1, true)]
        private int _targetClientID;
    }
}
