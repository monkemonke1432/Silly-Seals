using System;
using Normal.GorillaTemplate.Keyboard;
using Normal.GorillaTemplate.PlayerData;
using UnityEngine;

namespace Normal.GorillaTemplate.UI.Computer {
    /// <summary>
    /// A that tab that changes controls preferences.
    /// </summary>
    public class ControlsTab : Tab {
        /// <summary>
        /// The label that describes the speed keys for the current turn mode.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Text _speedInstructionsLabel;

        public override string tabName => "Controls";

        public override void NotifyVisible(bool visible) {
            base.NotifyVisible(visible);

            if (!visible) {
                return;
            }

            UpdateSpeedInstructionsLabel(LocalPlayerData.turnMode);
        }

        public override void NotifyButtonPressed(KeyboardButtonData data) {
            base.NotifyButtonPressed(data);

            if (data.Type == KeyboardButtonType.Symbol) {
                switch (data.Symbol) {
                    case "Q":
                        LocalPlayerData.turnMode = LocalPlayerData.TurnMode.Snap;
                        UpdateSpeedInstructionsLabel(LocalPlayerData.turnMode);
                        break;

                    case "W":
                        LocalPlayerData.turnMode = LocalPlayerData.TurnMode.Smooth;
                        UpdateSpeedInstructionsLabel(LocalPlayerData.turnMode);
                        break;

                    case "E":
                        LocalPlayerData.turnMode = LocalPlayerData.TurnMode.Disabled;
                        UpdateSpeedInstructionsLabel(LocalPlayerData.turnMode);
                        break;

                    default:
                        // Handle keys 0 to 9
                        if (int.TryParse(data.Symbol, out var integer)) {
                            switch (LocalPlayerData.turnMode) {
                                case LocalPlayerData.TurnMode.Snap:
                                    SetSnapTurnAngle(integer);
                                    break;

                                case LocalPlayerData.TurnMode.Smooth:
                                    SetSmoothTurnSpeed(integer);
                                    break;

                                case LocalPlayerData.TurnMode.Disabled:
                                    // Do nothing
                                    break;

                                default:
                                    throw new Exception($"Unexpected turn mode: {LocalPlayerData.turnMode}");
                            }
                        }
                        break;
                }
            }
        }

        private void UpdateSpeedInstructionsLabel(LocalPlayerData.TurnMode turnMode) {
            _speedInstructionsLabel.text = turnMode switch {
                LocalPlayerData.TurnMode.Snap   => "    0 to 9 - Turn angle",
                LocalPlayerData.TurnMode.Smooth => "    0 to 9 - Turn speed",
                LocalPlayerData.TurnMode.Disabled => "",
                _ => throw new Exception($"Unexpected turn mode: {turnMode}"),
            };
        }

        private void SetSnapTurnAngle(int integer) {
            // We ensure that the player can always eventually face the same
            // direction by only using divisors of 360:
            LocalPlayerData.snapTurnIncrement = integer switch {
                0 => 360f / 24f, // 15°
                1 => 360f / 16f, // 22.5°
                2 => 360f / 12f, // 30°
                3 => 360f /  9f, // 40°
                4 => 360f /  8f, // 45°
                5 => 360f /  7f, // 51.42°
                6 => 360f /  6f, // 60°
                7 => 360f /  5f, // 72°
                8 => 360f /  4f, // 90°
                9 => 360f /  3f, // 120°
                _ => throw new Exception($"Unexpected {integer}"),
            };
        }

        private void SetSmoothTurnSpeed(int integer) {
            // Add +1 to skip 0 (LocalPlayerData.TurnMode.Disabled is our equivalent of 0) and convert to 0-1 range
            var ratio = (integer + 1) / 10f;

            // Rescale to a more useful range
            var multiplier = Mathf.Lerp(0.4f, 10f, ratio);

            // Apply as a multiplier of the default value
            var speed = multiplier * 45f;

            LocalPlayerData.smoothTurnSpeed = speed;
        }
    }
}
