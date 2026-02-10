using Normal.Realtime;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Scales a transform based on a player's voice chat volume.
    /// </summary>
    public class VoiceScale : MonoBehaviour {
        /// <summary>
        /// The voice chat component.
        /// </summary>
        public RealtimeAvatarVoice voice;

        /// <summary>
        /// The transform to scale.
        /// </summary>
        public Transform destination;

        /// <summary>
        /// The minimum and maximum scale that the transform should have at
        /// the minimum and maximum voice volume levels.
        /// </summary>
        public Vector2 scaleMinMax = new Vector2(1f, 1.5f);

        // Cache to combine it with _scaleMinMax
        private Vector3 _initialScale;

        private void Awake() {
            _initialScale = destination.transform.localScale;
        }

        void Update() {
            if (voice == null || destination == null) {
                return;
            }

            // Get the voice volume
            float voiceVolume = voice.voiceVolume;

            float scaleFactor = Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, voiceVolume);

            // Apply the scale to the this game object
            destination.transform.localScale = _initialScale * scaleFactor;
        }
    }
}
