using UnityEngine;

namespace Normal.GorillaTemplate.Cosmetics {
    /// <summary>
    /// A component that assigns the bones from another <see cref="SkinnedMeshRenderer"/>.
    /// This allows us to animate rigged cosmetics (like a shirt or a coat) using the same
    /// Transforms that are animating the rest of the character.
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class SkinnedMeshBonesTransfer : MonoBehaviour {
        /// <summary>
        /// The original armature (typically inside a model asset file).
        /// </summary>
        public SkinnedMeshRenderer originalRig;

        /// <summary>
        /// The renderer whose animation we want to match.
        /// </summary>
        public SkinnedMeshRenderer source;

        [SerializeField]
        private SkinnedMeshRenderer _lastTransferredSource;

#if UNITY_EDITOR
        public bool IsTransferred() {
            if (originalRig == null || source == null || source.rootBone == null) {
                return false;
            }

            var destination = GetComponent<SkinnedMeshRenderer>();
            return destination.rootBone == source.rootBone && _lastTransferredSource == source;
        }

        [ContextMenu("Transfer Bones")]
        public void TransferBones() {
            if (originalRig == null) {
                Debug.LogError("The Original Rig prefab must be assigned.");
                return;
            }

            if (source == null) {
                Debug.LogError($"Source {nameof(SkinnedMeshRenderer)} must be assigned.");
                return;
            }

            var destination = GetComponent<SkinnedMeshRenderer>();
            if (destination.sharedMesh == null) {
                Debug.LogError($"The mesh must be assigned on the {gameObject.name} {nameof(SkinnedMeshRenderer)}.");
                return;
            }

            // Find the SkinnedMeshRenderer in the prefab that corresponds to destination
            if (originalRig.sharedMesh != destination.sharedMesh) {
                Debug.LogError($"The {nameof(originalRig)} mesh doesn't match the destination mesh.");
                return;
            }

            // Remap the bones by matching names
            var newBones = new Transform[originalRig.bones.Length];
            for (var i = 0; i < newBones.Length; i++) {
                var boneName = originalRig.bones[i].name;
                var newBone = FindChildByName(source.rootBone, boneName);

                if (newBone == null) {
                    Debug.LogWarning($"Bone '{boneName}' not found in source rig.");
                }

                newBones[i] = newBone;
            }

            // Assign bones
            destination.bones = newBones;
            destination.rootBone = source.rootBone;

            _lastTransferredSource = source;

            Debug.Log("Skinned mesh transferred successfully.");

            // Make sure the changes get saved
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private Transform FindChildByName(Transform parent, string childName) {
            var children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children) {
                if (child.name == childName) {
                    return child;
                }
            }

            return null;
        }
#endif
    }
}
