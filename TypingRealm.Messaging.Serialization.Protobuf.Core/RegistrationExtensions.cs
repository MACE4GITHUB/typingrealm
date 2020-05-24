using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Meta;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Registers protobuf stream connection factory and adds a post-build
        /// action to register all messages in protobuf RuntimeTypeModel.
        /// </summary>
        public static MessageTypeCacheBuilder AddProtobuf(this MessageTypeCacheBuilder builder)
        {
            builder.Services.AddTransient<IProtobufConnectionFactory, ProtobufConnectionFactory>();
            builder.AddPostBuildAction(messageTypes => RegisterProtobufMessages(messageTypes));

            return builder;
        }

        private static void RegisterProtobufMessages(IMessageTypeCache messageTypes)
        {
            foreach (var type in messageTypes.GetAllTypes())
            {
                RuntimeTypeModel.Default.Add(type.Value, false)
                    .Add(type.Value
                        .GetProperties()
                        .Select(property => property.Name)
                        .OrderBy(name => name)
                        .ToArray());
            }
        }
    }
}
