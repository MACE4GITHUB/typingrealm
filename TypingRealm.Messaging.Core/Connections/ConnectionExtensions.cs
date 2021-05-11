namespace TypingRealm.Messaging.Connections
{
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Adds locking to connection so that sending and receiving could be
        /// done only via single thread at a time.
        /// </summary>
        public static LockingConnection WithLocking(
            this IConnection connection, ILock sendLock, ILock receiveLock)
        {
            return new LockingConnection(connection, sendLock, receiveLock);
        }

        /// <summary>
        /// Adds receiving capabilities to <see cref="IMessageSender"/> so that
        /// it can work as <see cref="IConnection"/> using <see cref="Notificator"/>.
        /// </summary>
        public static NotificatorConnection WithNotificator(
            this IMessageSender messageSender, Notificator notificator)
        {
            return new NotificatorConnection(messageSender, notificator);
        }

        public static ReceivedAcknowledgingConnection WithReceiveAcknowledgement(
            this IConnection connection)
        {
            return new ReceivedAcknowledgingConnection(connection);
        }
    }
}
