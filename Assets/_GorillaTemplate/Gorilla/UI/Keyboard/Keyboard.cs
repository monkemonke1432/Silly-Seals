using UnityEngine;

namespace Normal.GorillaTemplate.Keyboard {
    /// <summary>
    /// A component designed to be inherited to handle button presses.
    /// Its buttons are assumed to be its children.
    /// </summary>
    public class Keyboard : MonoBehaviour {
        /// <summary>
        /// Dispatched when a user presses a button.
        /// </summary>
        /// <param name="data">Data about the button that was pressed.</param>
        public virtual void NotifyButtonPressed(KeyboardButtonData data) { }
    }
}
