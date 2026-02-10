using UnityEditor;
using UnityEngine;

namespace Normal.GorillaTemplate.Keyboard {
    /// <summary>
    /// A pressable button.
    /// It belongs to a parent <see cref="keyboard"/> component which
    /// is notified when this button is pressed.
    /// </summary>
    public class KeyboardButton : ButtonBase {
        /// <summary>
        /// The type of the button.
        /// </summary>
        [SerializeField]
        private KeyboardButtonType _type;

        /// <summary>
        /// For <see cref="KeyboardButtonType.Symbol"/>, the symbol that the button represents.
        /// </summary>
        [SerializeField]
        private string _symbol;

        private Keyboard _keyboard;

        /// <summary>
        /// The keyboard component that this button belongs to.
        /// </summary>
        public Keyboard keyboard => _keyboard;

        protected override void Awake() {
            base.Awake();
            _keyboard = GetComponentInParent<Keyboard>();
        }

        protected override void HandlePress() {
            var data = new KeyboardButtonData() {
                Type = _type,
                Symbol = _symbol,
            };

            _keyboard.NotifyButtonPressed(data);
        }

#if UNITY_EDITOR
        // A utility function to quickly update the values based on the GameObject name
        [ContextMenu("Set key from name")]
        protected virtual void SetKeyFromName() {
            var so = new SerializedObject(this);
            var symbolProperty = so.FindProperty(nameof(_symbol));
            symbolProperty.stringValue = name;
            so.ApplyModifiedProperties();

            var textLabel = GetComponentInChildren<TMPro.TMP_Text>();
            if (textLabel != null) {
                so = new SerializedObject(textLabel);
                var textProperty = so.FindProperty("m_text");
                textProperty.stringValue = name;
                so.ApplyModifiedProperties();
            }
        }
#endif
    }
}
