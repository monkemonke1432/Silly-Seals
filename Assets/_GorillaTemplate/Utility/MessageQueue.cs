using System.Collections;
using Normal.Realtime;
using Normal.Realtime.Serialization;
using UnityEngine;

namespace Normal.Utility {
    /// <summary>
    /// Implements a generic message queue on top of a RealtimeDictionary.
    /// Use this to send one-shot (transient) messages to other clients.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to pass in the queue.</typeparam>
    public abstract class MessageQueue<TMessage> : RealtimeComponent<MessageQueueModel<TMessage>>
        where TMessage : RealtimeModel, new() {
        /// <summary>
        /// Use this method to add a message to the queue.
        /// </summary>
        public void Send(TMessage message) {
            if (model == null) {
                return;
            }

            // Add to the queue
            model.messages.Add(_incrementingCounter, message);
            _incrementingCounter++;
        }

        /// <summary>
        /// Override this class to process newly added messages.
        /// </summary>
        /// <remarks>This is called both on local clients and remote clients.</remarks>
        protected abstract void Process(TMessage message);

        /// <summary>
        /// An incrementing counter to ensure that every entry in
        /// <see cref="MessageQueueModel{T}.messages"/> is unique.
        /// </summary>
        private uint _incrementingCounter;

        protected override void OnRealtimeModelReplaced(MessageQueueModel<TMessage> previousModel, MessageQueueModel<TMessage> currentModel) {
            if (previousModel != null) {
                previousModel.messages.modelAdded -= OnMessageAdded;
            }

            if (currentModel != null) {
                currentModel.messages.modelAdded += OnMessageAdded;
            }
        }

        private void OnMessageAdded(RealtimeDictionary<TMessage> dictionary, uint key, TMessage message, bool remote) {
            Process(message);

            if (!remote) {
                // The local client is responsible for cleaning up its own messages
                StartCoroutine(CleanupMessage(key));
            }
        }

        private IEnumerator CleanupMessage(uint key) {
            // Wait for the gamemode owner to receive the request
            yield return new WaitForSeconds(5f);

            // Cleanup the request
            model.messages.Remove(key);
        }
    }

    [RealtimeModel]
    public partial class MessageQueueModel<TMessage> where TMessage : RealtimeModel, new() {
        /// <summary>
        /// A dictionary used as a message queue.
        /// </summary>
        [RealtimeProperty(1, true)]
        private RealtimeDictionary<TMessage> _messages;
    }
}
