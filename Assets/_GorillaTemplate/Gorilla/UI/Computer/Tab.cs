using Normal.GorillaTemplate.Keyboard;
using UnityEngine;

namespace Normal.GorillaTemplate.UI.Computer {
    /// <summary>
    /// The base class for a tab inside a <see cref="Computer"/>.
    /// </summary>
    public abstract class Tab : MonoBehaviour {
        /// <summary>
        /// The name that will be displayed in the <see cref="TabHeader"/>.
        /// </summary>
        public abstract string tabName { get; }

        /// <summary>
        /// The computer that this tab belongs to.
        /// </summary>
        protected Computer computer { get; private set; }

        /// <summary>
        /// Called by <see cref="Computer"/>.
        /// </summary>
        public void Initialize(Computer comp) {
            computer = comp;
        }

        /// <summary>
        /// Invoked when the tab's visibility changes.
        /// </summary>
        /// <param name="visible">True if the tab is currently visible.</param>
        public virtual void NotifyVisible(bool visible) { }

        /// <summary>
        /// Invoked when the user presses a button on the computer.
        /// </summary>
        public virtual void NotifyButtonPressed(KeyboardButtonData data) { }
    }
}
