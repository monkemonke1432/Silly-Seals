using System;
using UnityEngine;

namespace Normal.GorillaTemplate.PlayerData {
    /// <summary>
    /// A class that saves player preferences to the local device (using Unity's <see cref="PlayerPrefs"/> API).
    /// </summary>
    public static class LocalPlayerData {
        public enum TurnMode {
            Snap     = 1,
            Smooth   = 2,
            Disabled = 3,
        }

        private const string __prefix                         = "LocalPlayerData";
        private static readonly string __playerNameKey        = $"{__prefix}/PlayerName";
        private static readonly string __turnModeKey          = $"{__prefix}/TurnMode";
        private static readonly string __snapTurnIncrementKey = $"{__prefix}/SnapTurnIncrement";
        private static readonly string __smoothTurnSpeedKey   = $"{__prefix}/SmoothTurnSpeed";
        private static readonly string __playerColorKey       = $"{__prefix}/PlayerColor";
        private static readonly string __playerColorRawKey    = $"{__prefix}/PlayerColorRaw";

        // Default values
        private static readonly string __defaultPlayerName         = string.Empty;
        private static readonly TurnMode __defaultTurnMode         = TurnMode.Snap;
        private static readonly float __defaultSnapTurnIncrement   = 30f;
        private static readonly float __defaultSmoothTurnIncrement = 45.0f;
        private static readonly Color __defaultPlayerColor         = new Color(0.1f, 1f, 0f, 1f);
        private static readonly Vector3Int __defaultPlayerColorRaw = new Vector3Int(1, 9, 0);

        /// <summary>
        /// Fired when the user changes any entry.
        /// </summary>
        public static Action onChanged;

        public static string playerName {
            get {
                return PlayerPrefs.GetString(__playerNameKey, __defaultPlayerName);
            }
            set {
                if (value != playerName) {
                    PlayerPrefs.SetString(__playerNameKey, value);
                    onChanged?.Invoke();
                }
            }
        }

        public static TurnMode turnMode {
            get {
                var rawValue = PlayerPrefs.GetInt(__turnModeKey, (int)__defaultTurnMode);

                // Use a validation check to avoid cast exceptions
                if (Enum.IsDefined(typeof(TurnMode), rawValue)) {
                    return (TurnMode)rawValue;
                } else {
                    return __defaultTurnMode;
                }
            }
            set {
                if (value != turnMode) {
                    PlayerPrefs.SetInt(__turnModeKey, (int)value);
                    onChanged?.Invoke();
                }
            }
        }

        public static float snapTurnIncrement {
            get {
                return PlayerPrefs.GetFloat(__snapTurnIncrementKey, __defaultSnapTurnIncrement);
            }
            set {
                if (Mathf.Approximately(value, snapTurnIncrement) == false) {
                    PlayerPrefs.SetFloat(__snapTurnIncrementKey, value);
                    onChanged?.Invoke();
                }
            }
        }

        public static float smoothTurnSpeed {
            get {
                return PlayerPrefs.GetFloat(__smoothTurnSpeedKey, __defaultSmoothTurnIncrement);
            }
            set {
                if (Mathf.Approximately(value, smoothTurnSpeed) == false) {
                    PlayerPrefs.SetFloat(__smoothTurnSpeedKey, value);
                    onChanged?.Invoke();
                }
            }
        }

        public static Vector3Int playerColorRaw {
            get {
                var r = PlayerPrefs.GetInt($"{__playerColorRawKey}.r", __defaultPlayerColorRaw.x);
                var g = PlayerPrefs.GetInt($"{__playerColorRawKey}.g", __defaultPlayerColorRaw.y);
                var b = PlayerPrefs.GetInt($"{__playerColorRawKey}.b", __defaultPlayerColorRaw.z);
                return new Vector3Int(r, g, b);
            }
            set {
                if (value != playerColorRaw) {
                    PlayerPrefs.SetInt($"{__playerColorRawKey}.r", value.x);
                    PlayerPrefs.SetInt($"{__playerColorRawKey}.g", value.y);
                    PlayerPrefs.SetInt($"{__playerColorRawKey}.b", value.z);
                    onChanged?.Invoke();
                }
            }
        }

        public static Color playerColor {
            get {
                var raw = (Vector3)playerColorRaw;

                // Divide by max raw value
                var ratio = raw / 9f;

                // Clamp to 0-1 range as a validation step
                return new Color(Mathf.Clamp01(ratio.x), Mathf.Clamp01(ratio.y), Mathf.Clamp01(ratio.z));
            }
        }
    }
}
