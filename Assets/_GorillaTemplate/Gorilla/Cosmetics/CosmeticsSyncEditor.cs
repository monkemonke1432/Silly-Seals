#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Normal.GorillaTemplate.Cosmetics {
    /// <summary>
    /// The editor interface for <see cref="CosmeticsSync"/>.
    /// </summary>
    [CustomEditor(typeof(CosmeticsSync))]
    public class CosmeticsSyncEditor : UnityEditor.Editor {
        private const string __fileExtension = "json";

        /// <summary>
        /// The instance that this editor is managing.
        /// </summary>
        public CosmeticsSync cosmeticsSync => (CosmeticsSync)target;

        public override void OnInspectorGUI() {
            serializedObject.UpdateIfRequiredOrScript();

            var layerProperty = serializedObject.FindProperty("_hideFromLocalCameraLayer");
            EditorGUILayout.PropertyField(layerProperty);
            EditorGUILayout.Space();

            if (cosmeticsSync.TryGetIsOwnedRemotelyInHierarchy(out var isOwnedRemotely) == false) {
                isOwnedRemotely = false;
            }

            if (isOwnedRemotely) {
                EditorGUILayout.HelpBox("This component is owned by a remote client, so cosmetics cannot be changed on the local client.", MessageType.Info);
            }

            if (GUILayout.Button("Save To File")) {
                SaveToFile();
            }

            // Only allow modifying cosmetics on the owner client
            using (new EditorGUI.DisabledScope(isOwnedRemotely)) {
                if (GUILayout.Button("Load From File")) {
                    foreach (var cosmetic in cosmeticsSync.cosmetics) {
                        Undo.RegisterFullObjectHierarchyUndo(cosmetic, "Load Cosmetics From File");
                    }

                    LoadFromFile();
                }

                // Display the list of cosmetics.
                // Each element in the list will be drawn by CosmeticEntryDrawer.
                var cosmeticsProperty = serializedObject.FindProperty("_cosmetics");
                EditorGUILayout.PropertyField(cosmeticsProperty);

                serializedObject.ApplyModifiedProperties();
            }

            if (!Application.isPlaying) {
                if (GUILayout.Button("Refresh Cosmetics List")) {
                    Undo.RecordObject(serializedObject.targetObject, "Refresh Cosmetics List");

                    cosmeticsSync.RefreshCosmeticsList();

                    EditorUtility.SetDirty(serializedObject.targetObject);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        /// <summary>
        /// Saves the state of the cosmetics to a JSON file.
        /// </summary>
        private void SaveToFile() {
            var path = EditorUtility.SaveFilePanel("Save cosmetics configuration to file", "Assets", $"Cosmetics Config.{__fileExtension}", __fileExtension);
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            var json = cosmeticsSync.SaveJson();

            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Loads the state of the cosmetics from a JSON file.
        /// </summary>
        private void LoadFromFile() {
            var path =  EditorUtility.OpenFilePanel("Load cosmetics configuration from file", "Assets", __fileExtension);
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            var json = File.ReadAllText(path);

            cosmeticsSync.TryLoadJson(json);
        }
    }

    /// <summary>
    /// The editor interface for each cosmetic in a <see cref="CosmeticsSync"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(CosmeticsSync.CosmeticEntry))]
    public class CosmeticEntryDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Get a direct reference to the cosmetic
            var cosmeticProperty = property.FindPropertyRelative("cosmetic");
            var cosmetic = cosmeticProperty.objectReferenceValue as Cosmetic;

            var cosmeticName = cosmetic == null ? " " : cosmetic.cosmeticName;

            // Display an object reference to the cosmetic, with its name as the label
            var cosmeticPropertyFieldPosition = position;
            cosmeticPropertyFieldPosition.width -= 20f;
            EditorGUI.PropertyField(cosmeticPropertyFieldPosition, cosmeticProperty, new GUIContent(cosmeticName));

            var togglePosition = position;
            togglePosition.x = cosmeticPropertyFieldPosition.xMax + 5f;
            togglePosition.width = 15f;

            // Display a toggle to show/hide the cosmetic
            var previousValue = cosmetic != null && cosmetic.cosmeticEnabled;
            var newValue = GUI.Toggle(togglePosition, previousValue, "");

            if (previousValue != newValue) {
                // Resolve the sync component (targetObject is the object being inspected by CosmeticsSyncEditor)
                var cosmeticsSync = property.serializedObject.targetObject as CosmeticsSync;

                if (cosmeticsSync != null) {
                    // Ensure Ctrl-Z works
                    Undo.RegisterFullObjectHierarchyUndo(cosmetic, "Toggle Cosmetic");

                    // Notify the sync component
                    cosmeticsSync.SetEnabled(cosmetic, newValue);

                    EditorUtility.SetDirty(cosmetic);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
#endif
