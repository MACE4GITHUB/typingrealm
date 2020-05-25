using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Connecting.Initializers;
using TypingRealm.Messaging.Handlers;
using TypingRealm.Messaging.Handling;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class TestMessageHandler : IMessageHandler<TestMessage>
    {
        public ValueTask HandleAsync(ConnectedClient sender, TestMessage message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class RegistrationExtensionsTests
    {
        private readonly IServiceProvider _provider;

        public RegistrationExtensionsTests()
        {
            _provider = new ServiceCollection()
                .AddLogging()
                .RegisterMessaging()
                .BuildServiceProvider();
        }

        [Theory]
        [InlineData(typeof(IConnectionInitializer), typeof(AnonymousConnectionInitializer))]
        [InlineData(typeof(IConnectedClientStore), typeof(ConnectedClientStore))]
        [InlineData(typeof(IConnectionHandler), typeof(ConnectionHandler))]
        [InlineData(typeof(IMessageDispatcher), typeof(MessageDispatcher))]
        [InlineData(typeof(IMessageHandlerFactory), typeof(MessageHandlerFactory))]
        [InlineData(typeof(IUpdater), typeof(AnnouncingUpdater))]
        [InlineData(typeof(IMessageHandler<Announce>), typeof(AnnounceHandler))]
        [InlineData(typeof(IMessageHandler<Disconnect>), typeof(DisconnectHandler))]
        public void ShouldRegisterTransientTypes(Type interfaceType, Type implementationType)
        {
            _provider.AssertRegisteredTransient(interfaceType, implementationType);
        }

        [Theory]
        [InlineData(typeof(IUpdateDetector), typeof(UpdateDetector))]
        public void ShouldRegisterSingletonTypes(Type interfaceType, Type implementationType)
        {
            _provider.AssertRegisteredSingleton(interfaceType, implementationType);
        }

        [Fact]
        public void ShouldRegisterHandler()
        {
            var provider = new ServiceCollection()
                .RegisterHandler<TestMessage, TestMessageHandler>()
                .BuildServiceProvider();

            provider.AssertRegisteredTransient(typeof(IMessageHandler<TestMessage>), typeof(TestMessageHandler));
        }
    }
}
