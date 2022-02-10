using TypingRealm.Messaging.Serialization.Connections;

namespace TypingRealm.Messaging.Serialization;

/// <summary>
/// These methods SHOULD be called on the lowest-level <see cref="IConnection"/>
/// instance that is getting raw <see cref="MessageData"/> objects.
/// </summary>
// TODO: Unit test this class.
public static class ConnectionExtensions
{
    /// <summary>
    /// Converts received <see cref="MessageData"/> objects to the actual messages,
    /// and converts actual messages to <see cref="MessageData"/> before sending.
    /// Attaches required metadata to objects before sending.
    /// </summary>
    public static IConnection AddCoreMessageSerialization(
        this IConnection connection,
        IMessageSerializer messageSerializer,
        IMessageTypeCache messageTypeCache,
        IMessageMetadataFactory messageMetadataFactory)
    {
        return new MessageSerializerConnection(
            connection,
            messageSerializer,
            messageTypeCache,
            messageMetadataFactory);
    }
}
