using UnityEngine;

namespace Normal.GorillaTemplate {
    [RequireComponent(typeof(GorillaAvatar))]
    public class PlayerModelSetup : MonoBehaviour {
        [SerializeField]
        private GameObject _playerModel;

        [SerializeField]
        private GameObject _rightHand;

        [SerializeField]
        private GameObject _leftHand;
    }
}
