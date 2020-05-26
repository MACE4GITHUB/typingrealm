using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Tests.SpecimenBuilders;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connecting
{
    public class ConnectedClientStoreExtensionsTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void IsClientConnected_ShouldBeConnected_WhenClientExists(
            ConnectedClient client,
            ConnectedClientStore sut)
        {
            sut.Add(client);

            Assert.True(sut.IsClientConnected(client.ClientId));
        }

        [Theory, AutoMoqData]
        public void IsClientConnected_ShouldNotBeConnected_WhenClientDoesNotExist(ConnectedClientStore sut)
        {
            Assert.False(sut.IsClientConnected(Create<string>()));
        }

        [Theory, AutoMoqData]
        public async Task SendAsync_ShouldSendMessageToAllClientsThatAreInSpecifiedGroup(
            CancellationToken ct,
            ConnectedClientStore sut)
        {
            var connections = Fixture.CreateMany<IConnection>(3).ToList();
            var clients = connections.Select(connection => WithConnection(connection)).ToList();
            foreach (var client in clients) { sut.Add(client); }
            clients[1].Group = clients[0].Group;

            var message = Create<TestMessage>();
            await sut.SendAsync(message, clients[0].Group, ct);

            Mock.Get(connections[0]).Verify(x => x.SendAsync(message, ct));
            Mock.Get(connections[1]).Verify(x => x.SendAsync(message, ct));
            Mock.Get(connections[2]).Verify(x => x.SendAsync(message, ct), Times.Never);
        }

        private ConnectedClient WithConnection(IConnection connection)
        {
            var builder = new ClientWithConnectionSpecimenBuilder(connection);

            Fixture.Customizations.Add(builder);
            var client = Fixture.Create<ConnectedClient>();
            Fixture.Customizations.Remove(builder);

            return client;
        }
    }
}
