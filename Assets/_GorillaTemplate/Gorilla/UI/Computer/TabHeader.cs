using UnityEngine;

namespace Normal.GorillaTemplate.UI.Computer {
    /// <summary>
    /// Represents a tab in the tab list on the computer.
    /// </summary>
    public class TabHeader : MonoBehaviour {
        /// <summary>
        /// The label that displays the tab name.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Text _label;

        /// <summary>
        /// The tab that this header represents.
        /// </summary>
        private Tab _tab;

        public void Initialize(Tab tab) {
            _tab = tab;
        }

        public void SetIsCurrentTab(bool isCurrentTab) {
            // If this is the current tab, display a ">" prefix in the header
            _label.text = isCurrentTab ? $"> {_tab.tabName}" : _tab.tabName;
        }
    }
}
