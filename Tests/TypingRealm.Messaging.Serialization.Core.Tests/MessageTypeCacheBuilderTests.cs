using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests
{
    public class MessageTypeCacheBuilderTests
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
            var cache1 = provider.GetRequiredService<IMessageTypeCache>();
            var cache2 = provider.GetRequiredService<IMessageTypeCache>();
            using var scope = provider.CreateScope();
            var cache3 = scope.ServiceProvider.GetRequiredService<IMessageTypeCache>();

            Assert.Equal(cache1, cache2);
            Assert.Equal(cache1, cache3);
        }

        [Fact]
        public void ShouldExecuteAllRegisteredPostBuildActions_InOrder_SingleTime_WhenFirstTimeRequired()
        {
            var services = new ServiceCollection();
            var builder = new MessageTypeCacheBuilder(services);
            var actions = new Dictionary<int, IMessageTypeCache>();
            builder.AddPostBuildAction(cache => actions.Add(1, cache));
            builder.AddPostBuildAction(cache => actions.Add(3, cache));
            builder.AddPostBuildAction(cache => actions.Add(2, cache));

            Assert.Empty(actions);

            var provider = services.BuildServiceProvider();
            Assert.Empty(actions);

            var cache = provider.GetRequiredService<IMessageTypeCache>();
            Assert.Equal(3, actions.Count);

            provider.GetRequiredService<IMessageTypeCache>();
            Assert.Equal(3, actions.Count);

            Assert.Equal(1, actions.Keys.ToList()[0]);
            Assert.Equal(3, actions.Keys.ToList()[1]);
            Assert.Equal(2, actions.Keys.ToList()[2]);
            Assert.Equal(cache, actions[1]);
            Assert.Equal(cache, actions[2]);
            Assert.Equal(cache, actions[3]);
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
