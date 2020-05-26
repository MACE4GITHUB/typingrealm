using System;
using System.Threading;
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
        public async Task ShouldCreateClientWithGuidIdentity(AnonymousConnectionInitializer sut)
        {
            var client = await sut.ConnectAsync(Create<IConnection>(), default);
            Assert.False(string.IsNullOrWhiteSpace(client.ClientId));
            Assert.NotEqual(Guid.Empty, Guid.Parse(client.ClientId));
        }

        [Theory, AutoMoqData]
        public async Task ShouldCreateUniqueIdentities(AnonymousConnectionInitializer sut)
        {
            var client1 = await sut.ConnectAsync(Create<IConnection>(), default);
            var client2 = await sut.ConnectAsync(Create<IConnection>(), default);

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
        public async Task ShouldSetGroupToLobby(AnonymousConnectionInitializer sut)
        {
            var client = await sut.ConnectAsync(Create<IConnection>(), default);
            Assert.Equal("Lobby", client.Group);
        }

        [Theory, AutoMoqData]
        public async Task ShouldNotUseCancellationToken(AnonymousConnectionInitializer sut)
        {
            Assert.NotNull(await sut.ConnectAsync(Create<IConnection>(), new CancellationToken(true)));
        }
    }
}
