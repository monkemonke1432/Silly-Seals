using System.Collections.Generic;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// Holds a reference to all <see cref="GrabbableObject">GrabbableObjects</see> in the room.
    /// Can be used to resolve a <see cref="GrabbableObject.networkID"/> to the corresponding <see cref="GrabbableObject"/> instance.
    /// </summary>
    [RequireComponent(typeof(Realtime.Realtime))]
    public class GrabManager : MonoBehaviour {
        private readonly Dictionary<string, GrabbableObject> _registry = new Dictionary<string, GrabbableObject>();

        private void Awake() {
            var realtime = GetComponent<Realtime.Realtime>();
            realtime.didDisconnectFromRoom += OnDisconnect;
        }

        public void _RegisterInternal(string networkID, GrabbableObject grabbableObject) {
            _registry.Add(networkID, grabbableObject);
        }

        public void _UnregisterInternal(string networkID) {
            _registry.Remove(networkID);
        }

        /// <summary>
        /// Resolves a network ID to an object instance, if it's registered locally.
        /// </summary>
        public bool TryGetGrabbableObject(string networkID, out GrabbableObject grabbableObject) {
            return _registry.TryGetValue(networkID, out grabbableObject);
        }

        private void OnDisconnect(Realtime.Realtime realtime) {
            _registry.Clear();
        }
    }
}
