using UnityEngine;
using UnityEngine.InputSystem;

namespace Normal.XR {
    /// <summary>
    /// A processor that can be added to an InputSystem stick action.
    /// It tweaks the responsiveness of the input to even it out across the stick's range of motion.
    /// </summary>
    public class XRSmoothTurnProcessor : InputProcessor<Vector2> {
#if UNITY_EDITOR
        static XRSmoothTurnProcessor() {
            Initialize();
        }
#endif

        [RuntimeInitializeOnLoadMethod]
        static void Initialize() {
            InputSystem.RegisterProcessor<XRSmoothTurnProcessor>();
        }

        /// <summary>
        /// Controls how much to boost the lower range of the stick.
        /// </summary>
        public float exponent = 1.5f;

        public override Vector2 Process(Vector2 value, InputControl control) {
            // Ignore the Y value for turn input.
            // Raise the X value to the exponent, but keep its original sign.
            value.x = Mathf.Pow(Mathf.Abs(value.x), exponent) * Mathf.Sign(value.x);

            return value;
        }
    }
}
