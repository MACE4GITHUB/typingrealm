using System;
using System.Collections.Concurrent;

namespace TypingRealm.Messaging.Connections
{
    /// <summary>
    /// Used to notify <see cref="NotificatorConnection"/> that message has been
    /// received. This is used when we need to have notifications about received
    /// messages from some external system and we can't simply wrap it into
    /// <see cref="IConnection"/>.
    /// </summary>
    public sealed class Notificator
    {
        /// <summary>
        /// The queue of already received messages. <see cref="NotificatorConnection"/>
        /// reads from this queue after it gets notification from
        /// <see cref="Received"/> event.
        /// </summary>
        internal ConcurrentQueue<object> ReceivedMessagesBuffer { get; }
            = new ConcurrentQueue<object>();

        /// <summary>
        /// <see cref="NotificatorConnection"/> subscribes to this event when
        /// we are waiting to receive a message. This event fires after received
        /// message has been put into <see cref="ReceivedMessagesBuffer"/> queue.
        /// </summary>
        internal event Action? Received;

        /// <summary>
        /// Notifies <see cref="NotificatorConnection"/> that the message has
        /// been received, supplying it the received message instance.
        /// </summary>
        /// <param name="message">Received message.</param>
        public void NotifyReceived(object message)
        {
            ReceivedMessagesBuffer.Enqueue(message);
            Received?.Invoke();
        }
    }
}
