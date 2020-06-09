using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Serialization.Json
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Registers <see cref="JsonConnectionFactory"/> that relies on
        /// <see cref="IMessageTypeCache"/>. <see cref="Serialization.RegistrationExtensions.AddSerializationCore(IServiceCollection)"/>
        /// should be called first.
        /// </summary>
        public static MessageTypeCacheBuilder AddJson(this MessageTypeCacheBuilder builder)
        {
            builder.Services.AddTransient<IJsonConnectionFactory, JsonConnectionFactory>();
            builder.AddMessageType(typeof(JsonSerializedMessage));

            return builder;
        }
    }
}
