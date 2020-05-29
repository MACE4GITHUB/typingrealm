namespace TypingRealm.Messaging.Serialization.Json
{
    public interface IJsonConnectionFactory
    {
        JsonConnection CreateJsonConnection(IConnection innerConnection);
    }
}
