using System.Collections;
using Normal.Realtime;
using UnityEngine;

namespace Normal.SeamlessRooms {
    /// <summary>
    /// Connects to a room asynchronously.
    /// Will seamlessly swap the room on the Realtime once the new room is connected and ready.
    /// </summary>
    [RequireComponent(typeof(Realtime.Realtime))]
    public class SeamlessRoomConnecter : MonoBehaviour {
        /// <summary>
        /// The Realtime instance that this component is managing.
        /// </summary>
        public Realtime.Realtime realtime => _realtime;

        /// <summary>
        /// The room that will be set on the Realtime once it's connected and ready.
        /// </summary>
        public Room roomInProgress => _roomInProgress;

        /// <inheritdoc cref="realtime"/>
        private Realtime.Realtime _realtime;

        /// <inheritdoc cref="roomInProgress"/>
        private Room _roomInProgress;

        private Coroutine _swapRoomsCoroutine;

        private void Awake() {
            _realtime = GetComponent<Realtime.Realtime>();
        }

        private void OnDestroy() {
            // Destroy _room in case we switch scenes while connecting
            DestroyRoomIfNeeded();
        }

        /// <inheritdoc cref="ConnectToRoom"/>
        public void Connect(string roomName, Room.ConnectOptions connectOptions = default) {
            if (_realtime.disconnected == false && roomName == _realtime.room.name) {
                Debug.Log($"Already connecting or connected to {roomName}, ignoring the {nameof(Connect)} call");
                return;
            }

            if (_roomInProgress != null && roomName == _roomInProgress.name) {
                Debug.Log($"Already connecting to {roomName}, ignoring the {nameof(Connect)} call");
                return;
            }

            // If we were still connecting to a different room, destroy it
            DestroyRoomIfNeeded();

            // Usually Realtime.Connect() will fill these in, so we must do it manually here:
            connectOptions.appKey ??= _realtime.normcoreAppSettings.normcoreAppKey;
            connectOptions.matcherURL ??= _realtime.normcoreAppSettings.matcherURL;

            // Create room
            _roomInProgress = new Room();

            // Subscribe to state changes
            _roomInProgress.connectionStateChanged += OnConnectionStateChanged;

            // Tell the room to connect
            _roomInProgress.Connect(roomName, connectOptions);
        }

        /// <summary>
        /// Connects to a room asynchronously.
        /// Will seamlessly swap the room on the Realtime once the new room is connected and ready.
        /// </summary>
        public void ConnectToRoom(string roomName) {
            // Connect using the default ConnectOptions
            Connect(roomName);
        }

        private void OnConnectionStateChanged(Room room, Room.ConnectionState previousState, Room.ConnectionState currentState) {
            if (currentState == Room.ConnectionState.Ready) {
                // Disconnect if necessary
                if (_realtime.connected) {
                    _realtime.Disconnect();
                }

                _swapRoomsCoroutine = StartCoroutine(SwapRooms(room));
            } else if (currentState == Room.ConnectionState.Disconnected || currentState == Room.ConnectionState.Error) {
                // The connection did not succeed, so we destroy the new room and stay connected to the current room
                DestroyRoomIfNeeded();
            }
        }

        private IEnumerator SwapRooms(Room room) {
            // Wait a frame to let OnDestroy() be called on objects from the old room we disconnected from.
            // This is particularly useful for RealtimeAvatar, otherwise the old avatars' OnDestroy()
            // and the new avatars' Start() calls can overlap and cause exceptions.
            yield return null;

            // Unsubscribe from future callbacks
            _roomInProgress.connectionStateChanged -= OnConnectionStateChanged;

            // We'll be assigning this room to the Realtime which will manage it going forward.
            // So we clear our reference (to avoid processing it inside our OnDestroy() or Update() functions).
            _roomInProgress = null;

            // Swap rooms
            _realtime.room = room;
        }

        private void Update() {
            if (_roomInProgress != null) {
                // Normally Realtime calls this while connecting, but we must do it
                // manually since the Room isn't assigned to a Realtime yet.
                _roomInProgress.Tick(Time.unscaledDeltaTime);
            }
        }

        private void DestroyRoomIfNeeded() {
            if (_swapRoomsCoroutine != null) {
                StopCoroutine(_swapRoomsCoroutine);
                _swapRoomsCoroutine = null;
            }

            if (_roomInProgress == null) {
                return;
            }

            _roomInProgress.connectionStateChanged -= OnConnectionStateChanged;
            _roomInProgress.Dispose();
            _roomInProgress = null;
        }
    }
}
