using System;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Settings that modify the throw velocities when a <see cref="GrabberHand"/> releases a <see cref="GrabbableObject"/>.
    /// </summary>
    [Serializable]
    public struct ThrowSettings {
        /// <summary>
        /// The default throw settings.
        /// </summary>
        /// <remarks>
        /// Default values for <see cref="maxLinearVelocityByWeight"/> and <see cref="maxAngularVelocityByWeight"/> are serialized in the "Gorilla Avatar" prefab.
        /// </remarks>
        public static readonly ThrowSettings defaultValue = new ThrowSettings() {
            linearVelocityMultiplier = 1f,
            angularVelocityMultiplier = 1f,
        };

        /// <summary>
        /// X axis: The <see cref="Rigidbody.mass"/> of the thrown <see cref="GrabbableObject"/>.
        /// Y axis: The maximum <see cref="Rigidbody.linearVelocity"/> that a hand can throw the object with.
        /// Think of this as the "muscle strength" of the character.
        /// See the default values in the "Gorilla Avatar" prefab.
        /// </summary>
        public AnimationCurve maxLinearVelocityByWeight;

        /// <summary>
        /// Multiplies the final linear velocity.
        /// </summary>
        public float linearVelocityMultiplier;

        /// <summary>
        /// X axis: The <see cref="Rigidbody.mass"/> of the thrown <see cref="GrabbableObject"/>.
        /// Y axis: The maximum <see cref="Rigidbody.angularVelocity"/> that a hand can throw the object with.
        /// Think of this as the "muscle strength" of the character.
        /// See the default values in the "Gorilla Avatar" prefab.
        /// </summary>
        /// <remarks>
        /// XR controllers are practically weightless, so players can reach angular velocities that may
        /// not be physically possible to do with heavier objects. Angular velocity can convert
        /// into linear velocity for objects that are held off-center, so it's important to carefully
        /// cap the angular velocity. Otherwise a heavy object could go flying from just a flick of the wrist.
        /// </remarks>
        public AnimationCurve maxAngularVelocityByWeight;

        /// <summary>
        /// Multiplies the final angular velocity.
        /// </summary>
        public float angularVelocityMultiplier;

        public struct Velocities {
            public Vector3 linearVelocity;
            public Vector3 angularVelocity;
        }

        /// <summary>
        /// Returns adjusted hand velocities for throwing an object,
        /// taking into account the object's weight and the hand's throw settings.
        /// </summary>
        /// <param name="grabbableObject">The object being thrown by the hand.</param>
        /// <param name="handVelocity">The hand velocities in world space.</param>
        public Velocities ModifyHandVelocitiesForThrow(GrabbableObject grabbableObject, Velocities handVelocity) {
            var mass = grabbableObject.physicsBody.mass;

            // Clamp linear velocity
            var maxLinearVelocity = maxLinearVelocityByWeight.Evaluate(mass);
            maxLinearVelocity *= grabbableObject.maxLinearVelocityMultiplier;
            var linearVelocity = Vector3.ClampMagnitude(handVelocity.linearVelocity, maxLinearVelocity);

            // Clamp angular velocity
            var maxAngularVelocity = maxAngularVelocityByWeight.Evaluate(mass);

            // Warn the user that the curve they made is being clamped by the Physics settings
            if (maxAngularVelocity > Physics.defaultMaxAngularSpeed) {
                Debug.LogWarning($"{nameof(maxAngularVelocityByWeight)} exceeds {nameof(Physics)}.{nameof(Physics.defaultMaxAngularSpeed)} ({Physics.defaultMaxAngularSpeed}) and will not be fully applied");
            }

            maxAngularVelocity *= grabbableObject.maxAngularVelocityMultiplier;
            var angularVelocity = Vector3.ClampMagnitude(handVelocity.angularVelocity, maxAngularVelocity);

            // Scale velocities
            linearVelocity *= linearVelocityMultiplier;
            angularVelocity *= angularVelocityMultiplier;

            return new Velocities() {
                linearVelocity = linearVelocity,
                angularVelocity = angularVelocity,
            };
        }
    }
}
