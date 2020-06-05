using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Domain.Messages;
using TypingRealm.Domain.Tests.Customizations;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Domain.Tests
{
    public class ConnectionInitializerTests : TestsBase
    {
        [Theory, AutoDomainData]
        public async Task ShouldThrow_WhenFirstMessageIsNotJoin(
            IConnection connection,
            ConnectionInitializer sut)
        {
            Mock.Get(connection).Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(Create<TestMessage>());

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await sut.ConnectAsync(connection, Cts.Token));
        }

        [Theory, AutoDomainData]
        public async Task ShouldThrow_WhenPlayerDoesNotExist(
            IConnection connection,
            PlayerId playerId,
            [Frozen]Mock<IPlayerRepository> playerRepository,
            ConnectionInitializer sut)
        {
            var joinMessage = Fixture.Build<Join>()
                .With(x => x.PlayerId, playerId)
                .Create();

            Mock.Get(connection).Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(joinMessage);

            playerRepository.Setup(x => x.Find(playerId))
                .Returns<Player>(null);

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await sut.ConnectAsync(connection, Cts.Token));
        }

        [Theory, AutoDomainData]
        public async Task ShouldUsePlayerIdAsClientId(
            IConnection connection,
            PlayerId playerId,
            ConnectionInitializer sut)
        {
            var joinMessage = Fixture.Build<Join>()
                .With(x => x.PlayerId, playerId)
                .Create();

            Mock.Get(connection).Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(joinMessage);

            var client = await sut.ConnectAsync(connection, Cts.Token);

            Assert.Equal(playerId, client.ClientId);
        }

        [Theory, AutoDomainData]
        public async Task ShouldSetConnectionAndUpdateDetector(
            IConnection connection,
            PlayerId playerId,
            [Frozen]IUpdateDetector updateDetector,
            ConnectionInitializer sut)
        {
            var joinMessage = Fixture.Build<Join>()
                .With(x => x.PlayerId, playerId)
                .Create();

            Mock.Get(connection).Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(joinMessage);

            var client = await sut.ConnectAsync(connection, Cts.Token);

            Assert.Equal(connection, client.Connection);
            Assert.Equal(updateDetector, GetPrivateField(client, "_updateDetector"));
        }

        [Theory, AutoDomainData]
        public async Task ShouldUsePlayerUniquePositionAsGroup(
            IConnection connection,
            Player player,
            [Frozen]Mock<IPlayerRepository> playerRepository,
            ConnectionInitializer sut)
        {
            var joinMessage = Fixture.Build<Join>()
                .With(x => x.PlayerId, player.PlayerId)
                .Create();

            Mock.Get(connection).Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(joinMessage);

            playerRepository.Setup(x => x.Find(player.PlayerId))
                .Returns(player);

            var client = await sut.ConnectAsync(connection, Cts.Token);

            Assert.Equal(player.GetUniquePlayerPosition(), client.Group);
        }
    }
}
