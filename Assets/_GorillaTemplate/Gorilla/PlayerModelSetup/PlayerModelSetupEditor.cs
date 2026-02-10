#if UNITY_EDITOR
using System.Linq;
using Normal.GorillaTemplate.Infection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace Normal.GorillaTemplate {
    [CustomEditor(typeof(PlayerModelSetup))]
    public class PlayerModelSetupEditor : UnityEditor.Editor {
        private SerializedProperty _playerModelProperty;
        private SerializedProperty _rightHandProperty;
        private SerializedProperty _leftHandProperty;

        private GameObject _newModelPrefab;

        private SerializedObject _gorillaAvatarSO;
        private SerializedProperty _headMeshProperty;

        private SerializedObject _gorillaSkinSO;
        private SerializedProperty _skinRenderersProperty;

        private const int __numSteps = 11;
        private readonly bool[] _stepExpanded = new bool[__numSteps];

        private GameObject rootGameObject => ((PlayerModelSetup)target).gameObject;
        private GameObject playerModel => (GameObject)_playerModelProperty.objectReferenceValue;

        private void OnEnable() {
            LoadExpandedState();

            _playerModelProperty = serializedObject.FindProperty("_playerModel");
            _rightHandProperty = serializedObject.FindProperty("_rightHand");
            _leftHandProperty = serializedObject.FindProperty("_leftHand");

            if (rootGameObject.TryGetComponent(out GorillaAvatar gorillaAvatar)) {
                _gorillaAvatarSO = new SerializedObject(gorillaAvatar);
                _headMeshProperty = _gorillaAvatarSO.FindProperty("_headMesh");
            }

            if (rootGameObject.TryGetComponent(out GorillaSkin gorillaSkin)) {
                _gorillaSkinSO = new SerializedObject(gorillaSkin);
                _skinRenderersProperty = _gorillaSkinSO.FindProperty("_targetRenderers");
            }
        }

        public override void OnInspectorGUI() {
            if (PrefabStageUtility.GetPrefabStage(rootGameObject) == null) {
                EditorGUILayout.HelpBox("This component is only visible in prefab editing mode.", MessageType.Info);
                return;
            }

            if (_headMeshProperty == null) {
                EditorGUILayout.HelpBox($"Missing {nameof(GorillaAvatar)} component.", MessageType.Info);
                return;
            }

            if (_skinRenderersProperty == null) {
                EditorGUILayout.HelpBox($"Missing {nameof(GorillaSkin)} component.", MessageType.Info);
                return;
            }

            serializedObject.UpdateIfRequiredOrScript();
            _gorillaAvatarSO.UpdateIfRequiredOrScript();
            _gorillaSkinSO.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(_playerModelProperty);
            if (playerModel != null) {
                var invalid = playerModel.transform.IsChildOf(rootGameObject.transform) == false;
                if (invalid) {
                    DrawError("Invalid player model selected. The player model must be inside the prefab.");
                }
            }
            EditorGUILayout.Space();

            GUILayout.Label($"Steps to swap the player model");

            var stepIndex = 0;
            StepDisableOldPlayerModel(stepIndex++);
            StepAddNewPlayerModel(stepIndex++);
            StepSetHeadMesh(stepIndex++);
            StepSetSkinMeshRenderers(stepIndex++);
            StepSetupArmsIK(stepIndex++);
            StepSetupKeyboardInteractors(stepIndex++);
            StepSetupFingerAnimations(stepIndex++);
            StepAdjustInfectionHitboxes(stepIndex++);
            StepAdjustNameTag(stepIndex++);
            StepAdjustSpeakerIcon(stepIndex++);
            StepDeleteOldPlayerModel(stepIndex++);

            serializedObject.ApplyModifiedProperties();
            _gorillaAvatarSO.ApplyModifiedProperties();
            _gorillaSkinSO.ApplyModifiedProperties();
        }

        private void StepDisableOldPlayerModel(int stepIndex) {
            if (DrawStepFoldout("Disable the old player model", stepIndex) == false) {
                return;
            }

            var disabled = playerModel == null;

            using (new EditorGUI.DisabledScope(disabled)) {
                if (GUILayout.Button("Disable")) {
                    Undo.RegisterFullObjectHierarchyUndo(playerModel, "Disabled old player model");
                    playerModel.SetActive(false);
                    MarkPrefabDirty();
                }
            }

            EditorGUILayout.Space();
        }

        private void StepAddNewPlayerModel(int stepIndex) {
            if (DrawStepFoldout("Add the new player model", stepIndex) == false) {
                return;
            }

            GUILayout.Label("Select the new model's prefab:");
            _newModelPrefab = (GameObject)EditorGUILayout.ObjectField(GUIContent.none, _newModelPrefab, typeof(GameObject), false);

            var disabled = _newModelPrefab == null;

            using (new EditorGUI.DisabledScope(disabled)) {
                if (GUILayout.Button("Add")) {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(_newModelPrefab, rootGameObject.transform);
                    if (instance != null) {
                        _playerModelProperty.objectReferenceValue = instance;
                        _playerModelProperty.serializedObject.ApplyModifiedProperties();

                        Undo.RegisterCreatedObjectUndo(instance, "Add an instance to this prefab");
                        MarkPrefabDirty();
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void StepSetHeadMesh(int stepIndex) {
            if (DrawStepFoldout("Set Head Mesh", stepIndex) == false) {
                return;
            }

            GUILayout.Label("Specify the Head Mesh GameObject from the player model:");
            EditorGUILayout.PropertyField(_headMeshProperty, GUIContent.none);

            if (_headMeshProperty.objectReferenceValue == null) {
                DrawError("No Head Mesh GameObject specified");
            } else if (playerModel != null) {
                var headMeshGO = (GameObject)_headMeshProperty.objectReferenceValue;
                if (BelongsToPlayerModel(headMeshGO.transform) == false) {
                    DrawError("The Head Mesh GameObject does not belong to the player model");
                }
            }

            EditorGUILayout.Space();
        }

        private void StepSetSkinMeshRenderers(int stepIndex) {
            if (DrawStepFoldout("Set Skin Mesh Renderers", stepIndex) == false) {
                return;
            }

            GUIStyle style = new GUIStyle(GUI.skin.label) {
                wordWrap = true,
            };
            GUILayout.Label("Specify the skin Renderers from the player model. Ex the ones on which the player color and the infection texture should be applied:", style);
            using (new EditorGUI.IndentLevelScope()) {
                EditorGUILayout.PropertyField(_skinRenderersProperty, new GUIContent("Renderers"));
            }

            if (_skinRenderersProperty.arraySize == 0) {
                DrawError("No Renderers specified");
            }

            if (playerModel != null) {
                for (var i = 0; i < _skinRenderersProperty.arraySize; i++) {
                    var meshRenderer = (Renderer)_skinRenderersProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (meshRenderer == null) {
                        DrawError("One of the renderers is null");
                    } else if (BelongsToPlayerModel(meshRenderer.transform) == false) {
                        DrawError("One of the renderers does not belong to the player model");
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void StepSetupArmsIK(int stepIndex) {
            if (DrawStepFoldout("Setup Arms IK", stepIndex) == false) {
                return;
            }

            var twoBoneIKConstraints = rootGameObject.GetComponentsInChildren<TwoBoneIKConstraint>();

            DrawConstraint("Right Arm IK");
            DrawConstraint("Left Arm IK");

            void DrawConstraint(string gameObjectName) {
                var constraint = twoBoneIKConstraints.FirstOrDefault(x => x.gameObject.name == gameObjectName);
                if (constraint == null) {
                    DrawError($"No Two Bone IK Constraint with name '{gameObjectName}' found");
                } else {
                    if (GUILayout.Button($"Inspect {gameObjectName}")) {
                        EditorGUIUtility.PingObject(constraint);
                    }

                    if (constraint.data.root == null || constraint.data.mid == null || constraint.data.tip == null) {
                        DrawError($"Missing target transforms on {gameObjectName}");
                    } else if (playerModel != null) {
                        if (BelongsToPlayerModel(constraint.data.root) == false || BelongsToPlayerModel(constraint.data.mid) == false || BelongsToPlayerModel(constraint.data.tip) == false) {
                            DrawError($"One of the target transforms on {gameObjectName} does not belong to the player model");
                        }
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void StepSetupKeyboardInteractors(int stepIndex) {
            if (DrawStepFoldout("Setup Keyboard Interactors", stepIndex) == false) {
                return;
            }

            var parentConstraints = rootGameObject.GetComponentsInChildren<ParentConstraint>();

            DrawConstraint("Right Keyboard Interactor Index");
            DrawConstraint("Right Keyboard Interactor Pinkie");
            DrawConstraint("Left Keyboard Interactor Index");
            DrawConstraint("Left Keyboard Interactor Pinkie");

            void DrawConstraint(string gameObjectName) {
                var constraint = parentConstraints.FirstOrDefault(x => x.gameObject.name == gameObjectName);
                if (constraint == null) {
                    DrawError($"No Parent Constraint with name '{gameObjectName}' found");
                } else {
                    if (GUILayout.Button($"Inspect {gameObjectName}")) {
                        EditorGUIUtility.PingObject(constraint);
                    }

                    for (var i = 0; i < constraint.sourceCount; i++) {
                        var source = constraint.GetSource(i);

                        if (source.sourceTransform == null) {
                            DrawError($"Missing source transforms on {gameObjectName}");
                        } else if (playerModel != null) {
                            if (BelongsToPlayerModel(source.sourceTransform) == false) {
                                DrawError($"One of the source transforms on {gameObjectName} does not belong to the player model");
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void StepSetupFingerAnimations(int stepIndex) {
            if (DrawStepFoldout("Setup finger animations", stepIndex) == false) {
                return;
            }

            if (playerModel == null) {
                DrawError($"No Player Model is currently set.");
                return;
            }

            var fingerPoseAnimators = rootGameObject.GetComponentsInChildren<FingerPoseAnimator>();
            DrawHand("right hand", "Right Finger Pose Animator", _rightHandProperty);
            DrawHand("left hand", "Left Finger Pose Animator", _leftHandProperty);

            void DrawHand(string handName, string fingerPoseAnimatorName, SerializedProperty property) {
                var fingerPoseAnimator = fingerPoseAnimators.FirstOrDefault(x => x.gameObject.name == fingerPoseAnimatorName);
                SerializedObject fingerPoseAnimatorSO;
                SerializedProperty fingerPoseAnimatorProperty;
                if (fingerPoseAnimator == null) {
                    DrawError($"No {fingerPoseAnimatorName} found");
                    return;
                } else {
                    fingerPoseAnimatorSO = new SerializedObject(fingerPoseAnimator);
                    fingerPoseAnimatorProperty = fingerPoseAnimatorSO.FindProperty("_animator");
                }

                EditorGUILayout.PropertyField(property);
                var handGO = (GameObject)property.objectReferenceValue;
                if (handGO == null) {
                    DrawError($"No {handName} specified");
                } else if (BelongsToPlayerModel(handGO.transform) == false) {
                    DrawError($"The selected {handName} does not belong to the player model");
                } else if (handGO.TryGetComponent(out Animator animator) == false) {
                    DrawError($"Missing {nameof(Animator)} component on {handName}");
                    if (GUILayout.Button($"Add {nameof(Animator)} component to {handName}")) {
                        Undo.AddComponent<Animator>(handGO);
                    }
                } else if (animator.runtimeAnimatorController == null) {
                    DrawError($"Missing Controller on {handName} {nameof(Animator)}");
                } else if (fingerPoseAnimatorProperty != null && fingerPoseAnimatorProperty.objectReferenceValue != animator) {
                    DrawError($"Different {nameof(Animator)} referenced by {fingerPoseAnimatorName}");
                    if (GUILayout.Button($"Fix reference")) {
                        fingerPoseAnimatorProperty.objectReferenceValue = animator;
                        fingerPoseAnimatorSO.ApplyModifiedProperties();
                    }
                } else {
                    EditorGUILayout.HelpBox("Double-check that the curve names inside the Animation Clips match the bones on the new player model.", MessageType.Info);
                }
            }

            EditorGUILayout.Space();
        }

        private void StepAdjustInfectionHitboxes(int stepIndex) {
            if (DrawStepFoldout("Adjust infection hitboxes", stepIndex) == false) {
                return;
            }

            var hitboxes = rootGameObject.GetComponentsInChildren<InfectionHitbox>();
            if (hitboxes.Length == 0) {
                GUILayout.Label($"No hitboxes in this prefab.");
                return;
            }

            GUILayout.Label($"Adjust the colliders on these hitboxes to match the player model:");

            foreach (var hitbox in hitboxes) {
                if (GUILayout.Button($"Inspect {hitbox.gameObject.name}")) {
                    EditorGUIUtility.PingObject(hitbox);
                }
            }

            EditorGUILayout.Space();
        }

        private void StepAdjustNameTag(int stepIndex) {
            if (DrawStepFoldout("Adjust Name Tag", stepIndex) == false) {
                return;
            }

            GUILayout.Label($"Adjust the Name Tag's position and rotation to match the player model:");

            if (GUILayout.Button("Inspect")) {
                if (rootGameObject.TryGetComponent(out PlayerDataSync component)) {
                    if (component.nameTagVisual != null) {
                        EditorGUIUtility.PingObject(component.nameTagVisual);
                    } else {
                        Debug.LogWarning($"No {nameof(PlayerDataSync.nameTagVisual)} assigned on the {nameof(PlayerDataSync)} component");
                    }
                } else {
                    Debug.LogWarning($"No {nameof(PlayerDataSync)} component found");
                }
            }

            EditorGUILayout.Space();
        }

        private void StepAdjustSpeakerIcon(int stepIndex) {
            if (DrawStepFoldout("Adjust Speaker Icon", stepIndex) == false) {
                return;
            }

            GUILayout.Label($"Adjust the Speaker Icon's position and rotation to match the player model:");

            if (GUILayout.Button("Inspect")) {
                if (rootGameObject.TryGetComponent(out VoiceScale component)) {
                    if (component.destination != null) {
                        EditorGUIUtility.PingObject(component.destination);
                    } else {
                        Debug.LogWarning($"No {nameof(VoiceScale.destination)} assigned on the {nameof(VoiceScale)} component");
                    }
                } else {
                    Debug.LogWarning($"No {nameof(VoiceScale)} component found");
                }
            }

            EditorGUILayout.Space();
        }

        private void StepDeleteOldPlayerModel(int stepIndex) {
            if (DrawStepFoldout("Delete the old player model", stepIndex) == false) {
                return;
            }

            GUIStyle style = new GUIStyle(GUI.skin.label) {
                wordWrap = true,
            };
            GUILayout.Label("If you have a backup, and feel confident about it, you can now delete the old player model.", style);

            EditorGUILayout.Space();
        }

#region Utilities

        private bool DrawStepFoldout(string stepTitle, int stepIndex) {
            _stepExpanded[stepIndex] = EditorGUILayout.Foldout(_stepExpanded[stepIndex], $"Step {stepIndex + 1}: {stepTitle}");
            SaveExpandedState();
            return _stepExpanded[stepIndex];
        }

        private bool BelongsToPlayerModel(Transform transform) {
            return transform.IsChildOf(playerModel.transform);
        }

        private void MarkPrefabDirty() {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null) {
                return;
            }

            EditorSceneManager.MarkSceneDirty(stage.scene);
        }

        private void DrawError(string errorMessage) {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
        }

        private const string __expandedPrefsKey = "Normcore Gorilla Template/Player Model Setup Editor/Expanded";

        private void SaveExpandedState() {
            var encoded = "";
            foreach (var expanded in _stepExpanded) {
                encoded += expanded ? '1' : '0';
            }
            EditorPrefs.SetString(__expandedPrefsKey, encoded);
        }

        private void LoadExpandedState() {
            var encoded = EditorPrefs.GetString(__expandedPrefsKey, string.Empty);
            for (var i = 0; i < encoded.Length && i < _stepExpanded.Length; i++) {
                var expanded = encoded[i] == '1';
                _stepExpanded[i] = expanded;
            }
        }

#endregion
    }
}
#endif
