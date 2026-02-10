using System;
using UnityEngine;

namespace Normal.Utility {
    /// <summary>
    /// A minimal implementation of the singleton for classes deriving from <see cref="MonoBehaviour"/>.
    /// </summary>
    public class MonoBehaviourSingleton<TSingleton> : MonoBehaviour where TSingleton : MonoBehaviour {
        /// <summary>
        /// The global instance.
        /// </summary>
        public static TSingleton instance { get; private set; }

        protected virtual void Awake() {
            RegisterAsSingleton();
        }

        protected virtual void OnDestroy() {
            UnregisterAsSingleton();
        }

        private void RegisterAsSingleton() {
            if (instance == this) {
                return;
            }

            if (instance != null) {
                throw new Exception("Another component has already registered as the singleton.");
            }

            instance = this as TSingleton;
        }

        private void UnregisterAsSingleton() {
            if (instance == this) {
                instance = null;
            }
        }
    }
}
