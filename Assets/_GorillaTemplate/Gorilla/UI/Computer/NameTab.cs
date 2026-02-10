using Normal.GorillaTemplate.Keyboard;
using Normal.GorillaTemplate.PlayerData;
using UnityEngine;

namespace Normal.GorillaTemplate.UI.Computer {
    /// <summary>
    /// A tab that sets the player name.
    /// </summary>
    public class NameTab : Tab {
        private const int __maxNameLength = 8;

        /// <summary>
        /// The text field that holds the player name.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_InputField _inputField;

        public override string tabName => "Name";

        public override void NotifyVisible(bool visible) {
            base.NotifyVisible(visible);

            if (!visible) {
                return;
            }

            _inputField.text = LocalPlayerData.playerName;
        }

        public override void NotifyButtonPressed(KeyboardButtonData data) {
            base.NotifyButtonPressed(data);

            switch (data.Type) {
                case KeyboardButtonType.Symbol:
                    AddSymbol(data.Symbol);
                    break;

                case KeyboardButtonType.Backspace:
                    DeleteSymbol();
                    break;

                case KeyboardButtonType.Enter:
                    Submit();
                    break;
            }
        }

        /// <summary>
        /// Adds a symbol to the player name.
        /// </summary>
        private void AddSymbol(string symbol) {
            if (_inputField.text.Length >= __maxNameLength)
                return;

            _inputField.text += symbol;
        }

        /// <summary>
        /// Removes the right-most symbol from the player name.
        /// </summary>
        private void DeleteSymbol() {
            var length = _inputField.text.Length;
            var newLength = Mathf.Max(0, length - 1);
            _inputField.text = _inputField.text.Substring(0, newLength);
        }

        private void Submit() {
            var playerName = _inputField.text;
            if (!ValidateName(playerName))
                return;

            LocalPlayerData.playerName = playerName;
        }

        private bool ValidateName(string playerName) {
            if (playerName.Length == 0)
                return false;

            return true;
        }
    }
}
