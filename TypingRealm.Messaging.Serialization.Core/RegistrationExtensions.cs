using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Messaging.Serialization
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Registers IMessageTypeCache as singleton and returns new instance of
        /// <see cref="MessageTypeCacheBuilder"/> class that has core Messaging
        /// messages registered in it. This builder will be used it the time
        /// when <see cref="MessageTypeCache"/> is first requested.
        /// </summary>
        public static MessageTypeCacheBuilder AddSerializationCore(this IServiceCollection services)
        {
            return new MessageTypeCacheBuilder(services)
                .AddMessageTypesFromAssembly(typeof(Disconnect).Assembly);
        }
    }
}
