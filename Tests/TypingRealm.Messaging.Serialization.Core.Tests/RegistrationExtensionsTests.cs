using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Messages;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests
{
    public class RegistrationExtensionsTests
    {
        [Fact]
        public void AddSerializationCore_ShouldSetValidServices()
        {
            var services = new ServiceCollection();
            var sut = services.AddSerializationCore();

            Assert.Equal(services, sut.Services);
        }

        [Fact]
        public void AddSerializationCore_ShouldAddAllMessagesFromMessagingAssemblyAndOnlyThem()
        {
            var services = new ServiceCollection();
            _ = services.AddSerializationCore();

            var messages = services.BuildServiceProvider()
                .GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(x => x.Value)
                .ToList();

            var asmMessages = typeof(Announce).Assembly
                .GetTypes()
                .Where(x => x.GetCustomAttribute<MessageAttribute>() != null)
                .ToList();

            Assert.Equal(asmMessages.Count, messages.Count);
            foreach (var asmMessage in asmMessages)
            {
                Assert.Contains(asmMessage, messages);
            }
        }
    }
}
