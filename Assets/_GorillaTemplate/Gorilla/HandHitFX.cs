using System;
using GorillaLocomotion;
using Normal.XR;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Fires effects such as haptics when the player's hands collide with a surface.
    /// </summary>
    public class HandHitFX : MonoBehaviour {
        /// <summary>
        /// Represents a player's hand.
        /// </summary>
        [Serializable]
        private struct Hand {
            /// <summary>
            /// The component that plays the haptics effect.
            /// </summary>
            public XRHaptics haptics;

            /// <summary>
            /// Last cached state.
            /// </summary>
            [NonSerialized]
            public bool previousWasTouching;

            /// <summary>
            /// For keeping track of the cooldown.
            /// </summary>
            [NonSerialized]
            public float lastHitTime;
        }

        /// <summary>
        /// The player locomotion script that provides hand data.
        /// </summary>
        [SerializeField]
        private Player _player;

        /// <summary>
        /// A cooldown time before another effect can be triggered on the same hand.
        /// </summary>
        [SerializeField]
        private float _cooldownSeconds = 0.25f;

        /// <summary>
        /// The left hand configuration.
        /// </summary>
        [SerializeField]
        private Hand _leftHand;

        /// <summary>
        /// The right hand configuration.
        /// </summary>
        [SerializeField]
        private Hand _rightHand;

        private void Update() {
            if (ShouldHit(ref _leftHand, _player.wasLeftHandTouching)) {
                DoHit(ref _leftHand);
            }

            if (ShouldHit(ref _rightHand, _player.wasRightHandTouching)) {
                DoHit(ref _rightHand);
            }

            _leftHand.previousWasTouching = _player.wasLeftHandTouching;
            _rightHand.previousWasTouching = _player.wasRightHandTouching;
        }

        private bool ShouldHit(ref Hand hand, bool currentlyTouching) {
            // Check if the hand has just touched something
            if (currentlyTouching && !hand.previousWasTouching) {
                // Check the cooldown
                var timeDifference = Time.time - hand.lastHitTime;
                return timeDifference > _cooldownSeconds;
            }

            return false;
        }

        private void DoHit(ref Hand hand) {
            hand.lastHitTime = Time.time;
            hand.haptics.Rumble();
        }
    }
}
