using TypingRealm.Domain.Messages;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Domain
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Adds all domain messages to <see cref="MessageTypeCacheBuilder"/>.
        /// </summary>
        public static MessageTypeCacheBuilder AddDomainCore(this MessageTypeCacheBuilder builder)
            => builder.AddMessageTypesFromAssembly(typeof(Join).Assembly);
    }
}
