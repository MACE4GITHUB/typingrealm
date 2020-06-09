namespace TypingRealm.Messaging.Serialization.Json
{
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="IConnection"/> decorator that will serialize and
        /// deserialize all messages to json, making <see cref="JsonSerializedMessage"/>.
        /// </summary>
        public static JsonConnection WithJson(this IConnection connection, IJsonConnectionFactory jsonConnectionFactory)
        {
            return jsonConnectionFactory.CreateJsonConnection(connection);
        }
    }
}
