using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connecting
{
    public class ConnectedClientStoreExtensionsTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void IsClientConnected_ShouldBeConnected(
            ConnectedClient client,
            ConnectedClientStore sut)
        {
            sut.Add(client);

            Assert.True(sut.IsClientConnected(client.ClientId));
        }

        [Theory, AutoMoqData]
        public void IsClientConnected_ShouldNotBeConnected(ConnectedClientStore sut)
        {
            Assert.False(sut.IsClientConnected(Create<string>()));
        }

        [Theory, AutoMoqData]
        public void FindInGroups_ShouldFindAllClientsInGroupsAsParams(ConnectedClientStore sut)
        {
            var clients = Fixture.CreateMany<ConnectedClient>(5).ToList();
            foreach (var client in clients) { sut.Add(client); }
            clients[1].Group = clients[0].Group;
            clients[3].Group = clients[2].Group;

            Assert.Equal(2, sut.FindInGroups(clients[0].Group).Count());
            Assert.Equal(4, sut.FindInGroups(clients[0].Group, clients[0].Group, clients[2].Group).Count());
        }

        [Theory, AutoMoqData]
        public async Task SendAsync_ShouldSendToAllClientsThatAreInGroups(ConnectedClientStore sut)
        {
            var connections = Fixture.CreateMany<Mock<IConnection>>(3).ToList();
            var clients = connections.Select(x => WithConnection(x.Object)).ToList();
            foreach (var client in clients) { sut.Add(client); }
            clients[1].Group = clients[0].Group;

            var message = Create<object>();
            await sut.SendAsync(message, clients[0].Group, default);

            connections[0].Verify(x => x.SendAsync(message, default));
            connections[1].Verify(x => x.SendAsync(message, default));
            connections[2].Verify(x => x.SendAsync(message, default), Times.Never);
        }

        private ConnectedClient WithConnection(IConnection connection)
        {
            return new ConnectedClient(
                Create<string>(),
                connection,
                Create<string>(),
                Create<IUpdateDetector>());
        }
    }
}
