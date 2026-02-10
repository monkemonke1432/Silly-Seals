using UnityEngine.Events;

namespace Normal.GorillaTemplate.Keyboard {
    /// <summary>
    /// A simple pressable button.
    /// </summary>
    public class SimpleButton : ButtonBase {
        public UnityEvent onPressed;

        protected override void HandlePress() {
            onPressed?.Invoke();
        }
    }
}
