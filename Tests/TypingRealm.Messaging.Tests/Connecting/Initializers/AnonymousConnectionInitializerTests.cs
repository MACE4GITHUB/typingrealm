using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using TypingRealm.Messaging.Connecting.Initializers;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connecting.Initializers
{
    public class AnonymousConnectionInitializerTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldCreateUniqueGuidIdentities(AnonymousConnectionInitializer sut)
        {
            var client1 = await sut.ConnectAsync(Create<IConnection>(), default);
            Assert.False(string.IsNullOrWhiteSpace(client1.ClientId));
            Assert.NotEqual(Guid.Empty, Guid.Parse(client1.ClientId));

            var client2 = await sut.ConnectAsync(Create<IConnection>(), default);
            Assert.False(string.IsNullOrWhiteSpace(client2.ClientId));
            Assert.NotEqual(Guid.Empty, Guid.Parse(client2.ClientId));

            Assert.NotEqual(client1.ClientId, client2.ClientId);
        }

        [Theory, AutoMoqData]
        public async Task ShouldSetConnection(
            IConnection connection,
            AnonymousConnectionInitializer sut)
        {
            var client = await sut.ConnectAsync(connection, default);
            Assert.Equal(connection, client.Connection);
        }

        [Theory, AutoMoqData]
        public async Task ShouldSetUpdateDetector(
            [Frozen]IUpdateDetector updateDetector,
            AnonymousConnectionInitializer sut)
        {
            var client = await sut.ConnectAsync(Create<IConnection>(), default);
            Assert.Equal(updateDetector, GetPrivateField(client, "_updateDetector"));
        }

        [Theory, AutoMoqData]
        public async Task ShouldSetGroupToLobbyByDefault(AnonymousConnectionInitializer sut)
        {
            var client = await sut.ConnectAsync(Create<IConnection>(), default);
            Assert.Equal("Lobby", client.Group);
        }
    }
}
