using System;
using System.Collections.Generic;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Detects the <see cref="GrabbableObject"/> that is closest to a <see cref="GrabberHand"/>.
    /// </summary>
    public class GrabbableObjectDetector {
        [Serializable]
        public struct Settings {
            public static readonly Settings defaultValue = new Settings() {
                radius = 0.2f,
                layerMask = -1,
            };

            /// <summary>
            /// The radius of the sphere overlap check.
            /// </summary>
            public float radius;

            /// <summary>
            /// The layers to include in the sphere overlap check.
            /// </summary>
            public LayerMask layerMask;
        }

        /// <summary>
        /// The object that the hand is currently hovering over, if any.
        /// </summary>
        public GrabbableObject hoveredObject => _hoveredObject;

        public delegate void HoverEventDelegate(GrabbableObject grabbableObject);

        /// <summary>
        /// Invoked when the hand starts hovering a new object.
        /// </summary>
        public event HoverEventDelegate onHoverEnter;

        /// <summary>
        /// Invoked when the hand stops hovering the previously hovered object.
        /// </summary>
        public event HoverEventDelegate onHoverExit;

        private readonly HashSet<GrabbableObject> _overlappedObjects = new HashSet<GrabbableObject>();
        private GrabbableObject _hoveredObject;

        private static readonly Collider[] _overlappedColliders = new Collider[16];

        /// <summary>
        /// Picks the best hover candidate.
        /// The result is stored in <see cref="hoveredObject"/>.
        /// Also invokes <see cref="onHoverExit"/> and <see cref="onHoverEnter"/>.
        /// </summary>
        public void UpdateHover(GrabberHand hand, Settings settings) {
            var previousHoveredObject = _hoveredObject;
            var newHoveredObject = GetBestHoverCandidate(hand, settings);

            var hasChanged = newHoveredObject != previousHoveredObject;
            if (hasChanged == false) {
                return;
            }

            _hoveredObject = newHoveredObject;

            if (previousHoveredObject != null) {
                onHoverExit?.Invoke(previousHoveredObject);
            }

            if (newHoveredObject != null) {
                onHoverEnter?.Invoke(newHoveredObject);
            }
        }

        /// <summary>
        /// Returns the best current candidate that the hand can grab, if any.
        /// </summary>
        private GrabbableObject GetBestHoverCandidate(GrabberHand hand, Settings settings) {
            UpdateSphereOverlap(hand, settings);

            // Check the hand state
            var handCanHover = hand.grabbedObject == null;
            if (handCanHover == false) {
                return null;
            }

            var hasHoveredObject = _hoveredObject != null;

            // There's no previously hovered object, so we can just pick the nearest one
            if (hasHoveredObject == false) {
                TryGetNearestCandidate(hand, out var bestCandidate, out var _);
                return bestCandidate;
            }

            // The object we were hovering previously is no longer available, so we can just pick the nearest one
            var objectCanBeHovered = ObjectCanBeHovered(hand, _hoveredObject) && _overlappedObjects.Contains(hoveredObject);
            if (objectCanBeHovered == false) {
                TryGetNearestCandidate(hand, out var bestCandidate, out var _);
                return bestCandidate;
            }

            // Check if there's a closer object than the one we were hovering previously
            if (TryGetNearestCandidate(hand, out var nearestCandidate, out var distance) && nearestCandidate != _hoveredObject) {
                var currentDistance = GetDistanceToHand(hand, _hoveredObject);

                // Favor the current object a little bit to avoid frequently switching hover objects
                currentDistance *= 0.9f;

                if (distance < currentDistance) {
                    return nearestCandidate;
                }
            }

            return _hoveredObject;
        }

        /// <summary>
        /// Updates <see cref="_overlappedObjects"/> so that it contains the objects that are close to the hand,
        /// as determined by the provided <paramref name="settings"/>.
        /// </summary>
        private void UpdateSphereOverlap(GrabberHand hand, Settings settings) {
            var numOverlaps = Physics.OverlapSphereNonAlloc(hand.transform.position, settings.radius, _overlappedColliders, settings.layerMask);

            _overlappedObjects.Clear();

            for (var i = 0; i < numOverlaps; i++) {
                var candidateCollider = _overlappedColliders[i];

                var candidate = candidateCollider.GetComponentInParent<GrabbableObject>();
                if (candidate != null) {
                    _overlappedObjects.Add(candidate);
                }
            }
        }

        /// <summary>
        /// Returns true if the specified object is available to be hovered and grabbed.
        /// </summary>
        private bool ObjectCanBeHovered(GrabberHand hand, GrabbableObject grabbableObject) {
            // Cannot hover an object that's already being held
            if (grabbableObject.grabberHand != null) {
                return false;
            }

            // The object refuses the hover attempt
            if (grabbableObject.CanBeGrabbed(hand) == false) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the nearest candidate that the hand can grab, if any.
        /// </summary>
        private bool TryGetNearestCandidate(GrabberHand hand, out GrabbableObject nearestCandidate, out float distance) {
            nearestCandidate = null;
            distance = float.MaxValue;

            foreach (var candidate in _overlappedObjects) {
                if (ObjectCanBeHovered(hand, candidate) == false) {
                    continue;
                }

                var candidateDistance = GetDistanceToHand(hand, candidate);

                if (candidateDistance < distance) {
                    distance = candidateDistance;
                    nearestCandidate = candidate;
                }
            }

            return nearestCandidate != null;
        }

        /// <summary>
        /// Returns the distance between the hand and the specified object.
        /// Uses the object's grab anchor (for best results).
        /// </summary>
        private float GetDistanceToHand(GrabberHand hand, GrabbableObject grabbableObject) {
            return Vector3.Distance(hand.transform.position, grabbableObject.effectiveGrabAnchor.position);
        }

        public void _NotifyGrabbableObjectDestroyedInternal(GrabbableObject grabbableObject) {
            if (grabbableObject == _hoveredObject) {
                _hoveredObject = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draws a visual representation of the detector when it's selected in the editor hierarchy.
        /// </summary>
        public void OnDrawGizmosSelected(GrabberHand hand, Settings settings) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hand.transform.position, settings.radius);
        }
#endif
    }
}
