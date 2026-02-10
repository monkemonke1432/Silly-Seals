using System;
using UnityEngine;

/// <summary>
/// Rendering a mirror is a relatively expensive operation, so we can optimize it by only
/// enabling the mirror when the player is nearby.
/// </summary>
public class MirrorActivator : MonoBehaviour {
    /// <summary>
    /// The mirror to activate/de-activate.
    /// </summary>
    [SerializeField]
    private Mirror _mirror;

    /// <summary>
    /// The color of the mirror surface when it's de-activated.
    /// </summary>
    [SerializeField]
    private Color _inactiveColor = Color.white;

    /// <summary>
    /// The transition duration between activated/de-activated colors.
    /// </summary>
    [SerializeField]
    private float _transitionDuration = 0.33f;

    /// <summary>
    /// When true, the mirror is never fully disabled and only the layer mask is cleared.
    /// This can prevent hitches on standalone XR platforms that are caused by the camera
    /// resources being destroyed and created again when the mirror is re-enabled.
    /// </summary>
    [SerializeField]
    private bool _disableUsingLayerMask = false;

    // Initial values: de-activated
    private float _alphaCurrent;
    private float _alphaStart;
    private float _alphaEnd;
    private float _timeElapsed;

    private LayerMask _reflectionLayerMask;

    private static readonly int __colorStrengthID = Shader.PropertyToID("_ColorStrength");

    private void Start() {
        _reflectionLayerMask = _mirror.reflectionMask;
    }

    /// <summary>
    /// Assumes a trigger collider is setup on this GameObject.
    /// This collider should belong to a layer that only reacts to the player.
    /// </summary>
    private void OnTriggerEnter(Collider other) {
        // Lerp from current alpha to 1
        _alphaStart = _alphaCurrent;
        _alphaEnd = 1f;
        _timeElapsed = 0f;
    }

    private void OnTriggerExit(Collider other) {
        // Lerp from current alpha to 0
        _alphaStart = _alphaCurrent;
        _alphaEnd = 0f;
        _timeElapsed = 0f;
    }

    private void Update() {
        // Lerp
        var lerpFactor = _timeElapsed / _transitionDuration;
        _alphaCurrent = Mathf.Lerp(_alphaStart, _alphaEnd, lerpFactor);
        _timeElapsed += Time.deltaTime;

        // Manage mirror
        var mirrorIsVisible = _alphaCurrent > 0.0f;
        if (_disableUsingLayerMask) {
            _mirror.reflectionMask = mirrorIsVisible ? _reflectionLayerMask : 0;
        } else {
            _mirror.enabled = mirrorIsVisible;
        }

        _mirror.reflectionMask = _alphaCurrent > 0.0f ? -1 : 0;
        _mirror.mirrorMaterial.color = _inactiveColor;
        _mirror.mirrorMaterial.SetFloat(__colorStrengthID, 1f - _alphaCurrent);
    }
}
