using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace Normal.XR {
    /// <summary>
    /// Sets Unity's FixedUpdate rate (which is the physics tick rate) to match the XR device's display refresh rate.
    /// This is the simplest and most reliable way to ensure smoothness and responsiveness in an XR app.
    /// </summary>
    /// <remarks>
    /// Only works when using the Oculus XR Plugin.
    /// </remarks>
    public class XRPhysicsTickRateManager : MonoBehaviour {
        /// <summary>
        /// When true, <see cref="_manualOverrideTickRate"/> will be used instead of the XR device's display refresh rate.
        /// </summary>
        [SerializeField]
        private bool _manualOverride;

        [SerializeField]
        private int _manualOverrideTickRate = 50;

        private readonly List<XRDisplaySubsystem> _displaySubsystems = new List<XRDisplaySubsystem>();

        /// <summary>
        /// Prevents spamming the warning.
        /// </summary>
        private bool _warnedAboutMultipleSubsystems;

#if !UNITY_EDITOR
        private void Awake() {
            var activeLoader = XRGeneralSettings.Instance.Manager.activeLoader;
            if (activeLoader == null) {
                Debug.LogError($"No XR loader found");
                enabled = false;
            } else if (activeLoader.name != "OculusLoader") {
                Debug.LogError($"{nameof(XRPhysicsTickRateManager)} is only compatible with the Oculus XR plugin (OculusLoader). Current loader: {activeLoader.name}");
                enabled = false;
            }
        }
#endif

        private void Update() {
            if (_manualOverride) {
                Time.fixedDeltaTime = 1f / _manualOverrideTickRate;
                return;
            }

            SubsystemManager.GetSubsystems(_displaySubsystems);
            var refreshRateDetected = false;

            foreach (var displaySubsystem in _displaySubsystems) {
                if (!displaySubsystem.running) {
                    return;
                }

                if (displaySubsystem.TryGetDisplayRefreshRate(out var refreshRate) && refreshRate > 0f) {
                    // Have we already found a device with a valid refresh rate?
                    if (refreshRateDetected) {
                        if (!_warnedAboutMultipleSubsystems) {
                            _warnedAboutMultipleSubsystems = true;
                            Debug.LogWarning($"Found more than 1 {nameof(XRDisplaySubsystem)}, detected refresh rate ");
                        }
                        break;
                    }

                    refreshRateDetected = true;
                    var newFixedDeltaTime = 1f / refreshRate;

                    if (Mathf.Approximately(newFixedDeltaTime, Time.fixedDeltaTime) == false) {
                        Time.fixedDeltaTime = 1f / refreshRate;
                        Debug.Log($"Set the FixedUpdate tick rate to {refreshRate} Hz");
                    }
                }
            }
        }
    }
}
