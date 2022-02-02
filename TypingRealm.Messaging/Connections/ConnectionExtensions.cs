namespace TypingRealm.Messaging.Connections;

public static class ConnectionExtensions
{
    public static ReceivedAcknowledgingConnection WithReceiveAcknowledgement(
        this IConnection connection)
    {
        return new ReceivedAcknowledgingConnection(connection);
    }
}
