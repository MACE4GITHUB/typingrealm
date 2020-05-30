using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization;
using Xunit;

namespace TypingRealm.Domain.Tests
{
    public class RegistrationExtensionsTests
    {
        [Fact]
        public void AddDomainCore_ShouldAddAllMessagesFromDomainAssemblyAndOnlyThem()
        {
            var services = new ServiceCollection();
            _ = new MessageTypeCacheBuilder(services)
                .AddDomainCore();

            var messages = services.BuildServiceProvider()
                .GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(x => x.Value)
                .ToList();

            var asmMessages = typeof(Join).Assembly
                .GetTypes()
                .Where(x => x.GetCustomAttribute<MessageAttribute>() != null)
                .ToList();

            Assert.Equal(asmMessages, messages);
        }
    }
}
