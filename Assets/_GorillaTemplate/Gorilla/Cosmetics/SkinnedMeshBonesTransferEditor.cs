#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Normal.GorillaTemplate.Cosmetics {
    [CustomEditor(typeof(SkinnedMeshBonesTransfer))]
    public class SkinnedMeshTransferEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            var transferComponent = (SkinnedMeshBonesTransfer)target;

            serializedObject.UpdateIfRequiredOrScript();

            var originalRigProperty = serializedObject.FindProperty(nameof(SkinnedMeshBonesTransfer.originalRig));
            EditorGUILayout.PropertyField(originalRigProperty);

            var sourceProperty = serializedObject.FindProperty(nameof(SkinnedMeshBonesTransfer.source));
            EditorGUILayout.PropertyField(sourceProperty);

            if (GUILayout.Button("Transfer Bones")) {
                transferComponent.TransferBones();
            }

            if (sourceProperty.objectReferenceValue != null) {
                var source = (SkinnedMeshRenderer)sourceProperty.objectReferenceValue;
                var transferred = transferComponent.IsTransferred();

                if (transferred) {
                    EditorGUILayout.HelpBox($"Bones have been transferred from {source.gameObject.name}.", MessageType.Info);
                } else {
                    EditorGUILayout.HelpBox($"Bones have not been transferred from {source.gameObject.name}.", MessageType.Warning);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
