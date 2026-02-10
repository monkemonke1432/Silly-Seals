using UnityEditor;

#if UNITY_EDITOR
namespace Normal.GorillaTemplate.Editor {
    /// <summary>
    /// A custom editor for <see cref="PlayerDataSync"/>.
    /// Changes that the user makes in this editor will be synced across the network.
    /// </summary>
    [CustomEditor(typeof(PlayerDataSync))]
    public class PlayerDataSyncEditor : UnityEditor.Editor {
        private PlayerDataSync _playerDataSync => (PlayerDataSync)target;

        public override void OnInspectorGUI() {
            var change = new EditorGUI.ChangeCheckScope();
            using (change) {
                base.OnInspectorGUI();
            }

            if (change.changed) {
                _playerDataSync.NotifyChangedLocally();
            }
        }
    }
}
#endif
