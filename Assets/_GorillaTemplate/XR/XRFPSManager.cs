using System;
using UnityEngine;

#if UNITY_ANDROID
using Unity.XR.Oculus;
using UnityEngine.XR.Management;
#endif

#pragma warning disable CS0162
namespace Normal.XR {
    /// <summary>
    /// Sets the display FPS on the current XR device.
    /// </summary>
    /// <remarks>
    /// Only works on Meta Quest and when using the Oculus XR Plugin.
    /// </remarks>
    public class XRFPSManager : MonoBehaviour {
        /// <summary>
        /// The display FPS that the device should use.
        /// </summary>
        [SerializeField]
#pragma warning disable CS0414
        private int _targetDisplayRefreshRate = 90;
#pragma warning restore CS0414

#if UNITY_ANDROID
        private float[] _availableRefreshRates = Array.Empty<float>();

        private void Awake() {
#if UNITY_EDITOR
            // The editor manages the FPS
            return;
#endif
            var activeLoader = XRGeneralSettings.Instance.Manager.activeLoader;
            if (activeLoader == null) {
                Debug.LogError($"No XR loader found");
                return;
            } else if (activeLoader.name != "OculusLoader") {
                Debug.LogError($"{nameof(XRFPSManager)} is only compatible with the Oculus XR plugin (OculusLoader). Current loader: {activeLoader.name}");
                return;
            }

            if (!Performance.TryGetAvailableDisplayRefreshRates(out _availableRefreshRates)) {
                Debug.LogError("Failed to get the available display refresh rates on this device");
                return;
            }

            var targetFPSIsAvailable = false;
            foreach (var rate in _availableRefreshRates) {
                if (Mathf.Approximately(rate, _targetDisplayRefreshRate)) {
                    targetFPSIsAvailable = true;
                    break;
                }
            }

            if (!targetFPSIsAvailable) {
                Debug.LogError($"The requested display refresh rate ({_targetDisplayRefreshRate}) is not available on the device");
                return;
            }

            if (!Performance.TrySetDisplayRefreshRate(_targetDisplayRefreshRate)) {
                Debug.LogError($"Failed to change the display refresh rate");
                return;
            }

            Application.targetFrameRate = _targetDisplayRefreshRate;
            Debug.Log($"Set the display refresh rate to {_targetDisplayRefreshRate} Hz");
        }
#else
        // Quest Link doesn't support the Performance.TrySetDisplayRefreshRate API.
        // The user must select the refresh rate in the PC Meta Quest Link app instead.
#endif
    }
}
#pragma warning restore CS0162
