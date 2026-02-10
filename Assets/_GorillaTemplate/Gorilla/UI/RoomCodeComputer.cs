using Normal.GorillaTemplate.Keyboard;
using UnityEngine;
using UnityEngine.Events;

namespace Normal.GorillaTemplate.UI {
    /// <summary>
    /// A keypad that connects to a room.
    /// </summary>
    public class RoomCodeComputer : Keyboard.Keyboard {
        private const int __maxCodeLength = 6;

        /// <summary>
        /// Dispatched when the user submits a room code.
        /// </summary>
        public UnityEvent<string> onSubmit;

        /// <summary>
        /// The text field that holds the room code.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_InputField _inputField;

        public override void NotifyButtonPressed(KeyboardButtonData data) {
            base.NotifyButtonPressed(data);

            if (data.Type == KeyboardButtonType.Symbol) {
                AddSymbol(data.Symbol);
            } else if (data.Type == KeyboardButtonType.Backspace) {
                DeleteSymbol();
            } else if (data.Type == KeyboardButtonType.Enter) {
                Submit();
            }
        }

        /// <summary>
        /// Adds a symbol to the room code.
        /// </summary>
        private void AddSymbol(string symbol) {
            if (_inputField.text.Length >= __maxCodeLength)
                return;

            _inputField.text += symbol;
        }

        /// <summary>
        /// Removes the right-most symbol from the room code.
        /// </summary>
        private void DeleteSymbol() {
            var length = _inputField.text.Length;
            var newLength = Mathf.Max(0, length - 1);
            _inputField.text = _inputField.text.Substring(0, newLength);
        }

        /// <summary>
        /// Submits a room code to connect to it.
        /// </summary>
        private void Submit() {
            var roomCode = _inputField.text;
            if (!ValidateRoomCode(roomCode))
                return;

            onSubmit?.Invoke(roomCode);
        }

        private bool ValidateRoomCode(string roomCode) {
            if (roomCode.Length == 0)
                return false;

            return true;
        }
    }
}
