using System.Linq;
using System.Reflection;

namespace TypingRealm.Messaging.Serialization
{
    public static class MessageTypeCacheBuilderExtensions
    {
        /// <summary>
        /// Scans the assembly for types marked with <see cref="MessageAttribute"/>
        /// attribute and adds them to the cache builder.
        /// </summary>
        /// <param name="builder">Cache builder where to add message types.</param>
        /// <param name="assembly">Assembly to scan for message types.</param>
        public static MessageTypeCacheBuilder AddMessageTypesFromAssembly(
            this MessageTypeCacheBuilder builder, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t => t.GetCustomAttribute<MessageAttribute>() != null))
            {
                builder.AddMessageType(type);
            }

            return builder;
        }
    }
}
