using System;
using Normal.GorillaTemplate.Keyboard;
using UnityEngine.Events;

namespace Normal.GorillaTemplate.UI {
    /// <summary>
    /// A board where each button represents a room.
    /// </summary>
    public class RoomBoard : Keyboard.Keyboard {
        /// <summary>
        /// Dispatched when the user picks a room.
        /// </summary>
        public UnityEvent<string> onSubmit;

        public override void NotifyButtonPressed(KeyboardButtonData data) {
            base.NotifyButtonPressed(data);

            if (data.Type == KeyboardButtonType.Symbol) {
                onSubmit?.Invoke(data.Symbol);
            } else {
                throw new Exception($"Unexpected button type: {data.Type}");
            }
        }
    }
}
