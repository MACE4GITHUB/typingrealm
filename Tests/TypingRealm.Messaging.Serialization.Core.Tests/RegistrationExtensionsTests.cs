using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Messages;
using TypingRealm.Testing;
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
                .OrderBy(x => x.Name)
                .ToList();

            var asmMessages = typeof(Announce).Assembly
                .GetTypes()
                .Where(x => x.GetCustomAttribute<MessageAttribute>() != null)
                .OrderBy(x => x.Name)
                .ToList();

            Assert.Equal(asmMessages, messages);
        }

        [Fact]
        public void AddSerializationCore_ShouldRegisterMessageIdFactoryAsSingleton()
        {
            var services = new ServiceCollection();
            var provider = services.AddSerializationCore().Services.BuildServiceProvider();

            provider.AssertRegisteredSingleton<IMessageIdFactory, MessageIdFactory>();
        }

        [Fact]
        public void AddSerializationCore_ShouldRegisterMetadataFactoryAsTransient()
        {
            var services = new ServiceCollection();
            var provider = services.AddSerializationCore().Services.BuildServiceProvider();

            provider.AssertRegisteredTransient<IClientToServerMessageMetadataFactory, ClientToServerMessageMetadataFactory>();
        }
    }
}
