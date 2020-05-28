using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests
{
    public class MessageTypeCacheBuilderTests : TestsBase
    {
        [Fact]
        public void ShouldAssignServiceCollectionToProperty()
        {
            var services = new ServiceCollection();
            var sut = new MessageTypeCacheBuilder(services);

            Assert.Equal(services, sut.Services);
        }

        [Fact]
        public void ShouldRegisterMessageTypeCacheAsSingleton()
        {
            var services = new ServiceCollection();
            _ = new MessageTypeCacheBuilder(services);

            var provider = services.BuildServiceProvider();
            provider.AssertRegisteredSingleton<IMessageTypeCache, MessageTypeCache>();
        }

        [Fact]
        public void ShouldHaveNoMessagesByDefault()
        {
            var services = new ServiceCollection();
            _ = new MessageTypeCacheBuilder(services);

            var cache = services.BuildServiceProvider().GetRequiredService<IMessageTypeCache>();
            Assert.Empty(cache.GetAllTypes());
        }

        [Fact]
        public void ShouldAddMessages()
        {
            var services = new ServiceCollection();
            _ = new MessageTypeCacheBuilder(services)
                .AddMessageType(typeof(TestMessage))
                .AddMessageType(typeof(string))
                .AddMessageType(typeof(object));

            var cache = services.BuildServiceProvider().GetRequiredService<IMessageTypeCache>();
            Assert.Equal(3, cache.GetAllTypes().Count());
            Assert.Contains(typeof(TestMessage), cache.GetAllTypes().Select(x => x.Value));
            Assert.Contains(typeof(string), cache.GetAllTypes().Select(x => x.Value));
            Assert.Contains(typeof(object), cache.GetAllTypes().Select(x => x.Value));
        }
    }
}
