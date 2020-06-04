using System;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Domain.Combat;
using TypingRealm.Domain.Messages;
using TypingRealm.Domain.Movement;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Domain.Tests
{
    public class RegistrationExtensionsTests : TestsBase
    {
        private readonly IServiceProvider _provider;

        public RegistrationExtensionsTests()
        {
            _provider = new ServiceCollection()
                .AddSingleton(Create<IUpdateDetector>())
                .AddSingleton(Create<IPlayerRepository>())
                .AddSingleton(Create<ILocationStore>())
                .AddSingleton(Create<IConnectedClientStore>())
                .AddDomain()
                .BuildServiceProvider();
        }

        [Theory]
        [InlineData(typeof(IConnectionInitializer), typeof(ConnectionInitializer))]
        [InlineData(typeof(IPlayerFactory), typeof(PlayerFactory))]
        [InlineData(typeof(IUpdater), typeof(Updater))] // UseUpdateFactory.
        [InlineData(typeof(IUpdateFactory), typeof(UpdateFactory))]
        [InlineData(typeof(IMessageHandler<MoveToLocation>), typeof(MoveToLocationHandler))]
        [InlineData(typeof(IMessageHandler<Attack>), typeof(AttackHandler))]
        public void ShouldRegisterTransientTypes(Type interfaceType, Type implementationType)
        {
            _provider.AssertRegisteredTransient(interfaceType, implementationType);
        }
    }
}
