namespace Normal.GorillaTemplate.Wardrobe {
    /// <summary>
    /// A wardrobe that displays items that the user can browse and equip.
    /// <see cref="WardrobeItem"/> will register to the closest
    /// <see cref="IWardrobe"/> instance in the hierarchy.
    /// </summary>
    public interface IWardrobe {
        /// <summary>
        /// Registers an item with this wardrobe.
        /// Usually called automatically.
        /// </summary>
        void RegisterItem(WardrobeItem item);
    }
}
