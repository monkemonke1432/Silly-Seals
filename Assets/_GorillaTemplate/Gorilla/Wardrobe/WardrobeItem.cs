using System.Collections.Generic;
using UnityEngine;

namespace Normal.GorillaTemplate.Wardrobe {
    /// <summary>
    /// An item that the user can browse or equip using an <see cref="IWardrobe"/>.
    /// This component should be placed on the GameObject that represents the item,
    /// and which should be a child (direct or indirect) of the <see cref="IWardrobe"/> instance.
    /// </summary>
    public class WardrobeItem : MonoBehaviour {
        /// <summary>
        /// This item's name.
        /// By default this is the GameObject's name.
        /// </summary>
        public string wardrobeItemName => gameObject.name;

        /// <summary>
        /// The names of the cosmetics that should be enabled when the user equips this item using the wardrobe.
        /// </summary>
        public IEnumerable<string> cosmeticNames {
            get {
                if (_cosmeticNameOverrides.Count > 0) {
                    foreach (var nameOverride in _cosmeticNameOverrides) {
                        yield return nameOverride;
                    }
                } else {
                    yield return wardrobeItemName;
                }
            }
        }

        /// <summary>
        /// The names of the cosmetics to enable when the user equips this item using the wardrobe.
        /// If this list is left empty, then <see cref="wardrobeItemName"/> is used instead.
        /// </summary>
        /// <remarks>
        /// This field is useful for setting up ex more than one symmetrical cosmetic on each arm using a single wardrobe item.
        /// Ex "Wristband Left" and "Wristband Right" cosmetics for a wardrobe item simply named "Wristband".
        /// </remarks>
        [SerializeField]
        private List<string> _cosmeticNameOverrides = new List<string>();

        [Header("PlayFab")]

        [SerializeField]
        [Tooltip("The PlayFab catalog version that the item belongs to.")]
        private string _catalog = "Cosmetics";
        [SerializeField]
        [Tooltip("The ID of the item in the PlayFab catalog. If specified, the item must belong to the player's inventory to be wearable.")]
        private string _itemId;

        /// <summary>
        /// The PlayFab catalog version that the item belongs to.
        /// </summary>
        public string catalog => _catalog;

        /// <summary>
        /// The ID of the item in the PlayFab catalog.
        /// If specified, the item must belong to the player's inventory to be wearable.
        /// </summary>
        public string itemId => _itemId;

        private void Awake() {
            var wardrobeComputer = GetComponentInParent<IWardrobe>();
            if (wardrobeComputer == null) {
                Debug.LogError($"Failed to register wardrobe item: no {nameof(IWardrobe)} found in parent.");
                return;
            }

            wardrobeComputer.RegisterItem(this);
        }
    }
}
