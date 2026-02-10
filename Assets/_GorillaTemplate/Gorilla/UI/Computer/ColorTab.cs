using Normal.GorillaTemplate.Keyboard;
using Normal.GorillaTemplate.PlayerData;
using UnityEngine;
using UnityEngine.UI;

namespace Normal.GorillaTemplate.UI.Computer {
    /// <summary>
    /// A tab that sets the player color.
    /// </summary>
    public class ColorTab : Tab {
        private enum ColorComponent {
            Red,
            Green,
            Blue,
        }

        [SerializeField]
        private TMPro.TMP_Text _currentColorLabel;

        [SerializeField]
        private Image _currentColorImage;

        public override string tabName => "Color";

        private ColorComponent? _currentColorComponent;

        public override void NotifyVisible(bool visible) {
            base.NotifyVisible(visible);

            if (!visible) {
                return;
            }

            _currentColorComponent = null;
            UpdateColorUI();
        }

        public override void NotifyButtonPressed(KeyboardButtonData data) {
            base.NotifyButtonPressed(data);

            if (data.Type == KeyboardButtonType.Symbol) {
                switch (data.Symbol) {
                    case "Q":
                        _currentColorComponent = ColorComponent.Red;
                        break;

                    case "W":
                        _currentColorComponent = ColorComponent.Green;
                        break;

                    case "E":
                        _currentColorComponent = ColorComponent.Blue;
                        break;

                    default:
                        if (_currentColorComponent.HasValue && int.TryParse(data.Symbol, out var integer)) {
                            LocalPlayerData.playerColorRaw = ChangeComponent(LocalPlayerData.playerColorRaw, _currentColorComponent.Value, integer);
                            UpdateColorUI();
                        }
                        break;
                }
            }
        }

        private void UpdateColorUI() {
            var colorRaw = LocalPlayerData.playerColorRaw;
            _currentColorLabel.text = $"Red / Green / Blue: {colorRaw.x} / {colorRaw.y} / {colorRaw.z}";

            _currentColorImage.color = LocalPlayerData.playerColor;
        }

        private static Vector3Int ChangeComponent(Vector3Int color, ColorComponent colorComponent, int value) {
            switch (colorComponent) {
                case ColorComponent.Red:
                    color.x = value;
                    break;
                case ColorComponent.Green:
                    color.y = value;
                    break;
                case ColorComponent.Blue:
                    color.z = value;
                    break;
            }
            return color;
        }
    }
}
