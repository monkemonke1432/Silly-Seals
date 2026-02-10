using UnityEngine;
using UnityEngine.UI;

namespace Normal.GorillaTemplate.Keyboard {
    /// <summary>
    /// Animates a progress fill image for a button that uses <see cref="ButtonBase.TriggerMode.LongPress"/>.
    /// </summary>
    [RequireComponent(typeof(ButtonBase))]
    public class LongPressProgress : MonoBehaviour {
        [SerializeField]
        private Image _progressSprite;

        private ButtonBase _button;

        private void Awake() {
            _button = GetComponent<ButtonBase>();
        }

        private void LateUpdate() {
            if (_button.triggerMode != ButtonBase.TriggerMode.LongPress) {
                _progressSprite.enabled = false;
                _progressSprite.fillAmount = 0f;
                return;
            }

            _progressSprite.enabled = true;
            _progressSprite.fillAmount = Mathf.Clamp01(_button.longPressProgress);
        }
    }
}
