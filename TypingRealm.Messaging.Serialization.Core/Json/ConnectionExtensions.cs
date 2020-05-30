namespace TypingRealm.Messaging.Serialization.Json
{
    public static class ConnectionExtensions
    {
        public static JsonConnection WithJson(this IConnection connection, IJsonConnectionFactory jsonConnectionFactory)
        {
            return jsonConnectionFactory.CreateJsonConnection(connection);
        }
    }
}
