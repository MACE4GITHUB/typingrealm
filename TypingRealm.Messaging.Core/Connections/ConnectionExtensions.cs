namespace TypingRealm.Messaging.Connections
{
    public static class ConnectionExtensions
    {
        public static IConnection WithLocking(this IConnection connection, ILock sendLock, ILock receiveLock)
        {
            return new LockingConnection(connection, sendLock, receiveLock);
        }
    }
}
