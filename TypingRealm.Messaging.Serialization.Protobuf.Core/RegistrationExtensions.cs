using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Registers protobuf stream connection factory.
        /// </summary>
        public static IServiceCollection AddProtobuf(this IServiceCollection services)
        {
            services.AddTransient<IProtobufConnectionFactory, ProtobufConnectionFactory>();
            services.AddSingleton<IProtobuf>(provider =>
            {
                var messageTypeCache = provider.GetRequiredService<IMessageTypeCache>();
                var types = messageTypeCache.GetAllTypes().Select(idToType => idToType.Value);

                return new Protobuf(types);
            });

            return services;
        }
    }
}
