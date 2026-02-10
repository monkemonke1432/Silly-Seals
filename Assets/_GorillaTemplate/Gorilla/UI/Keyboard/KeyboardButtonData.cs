namespace Normal.GorillaTemplate.Keyboard {
    /// <summary>
    /// Data associated with a button.
    /// </summary>
    public struct KeyboardButtonData {
        /// <summary>
        /// The type of button.
        /// </summary>
        public KeyboardButtonType Type;

        /// <summary>
        /// For a button of type <see cref="KeyboardButtonType.Symbol"/>, the specific symbol that it represents.
        /// </summary>
        public string Symbol;
    }
}
