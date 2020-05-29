namespace TypingRealm.Messaging.Serialization.Json
{
    public sealed class JsonConnectionFactory : IJsonConnectionFactory
    {
        private readonly IMessageTypeCache _messageTypes;

        public JsonConnectionFactory(IMessageTypeCache messageTypes)
        {
            _messageTypes = messageTypes;
        }

        public JsonConnection CreateJsonConnection(IConnection innerConnection)
        {
            return new JsonConnection(innerConnection, _messageTypes);
        }
    }
}
