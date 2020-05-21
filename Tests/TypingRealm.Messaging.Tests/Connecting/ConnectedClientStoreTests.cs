using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connecting
{
    public class ConnectedClientStoreTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void ShouldAddClient_WhenNotAdded(
            [Frozen]IUpdateDetector updateDetector,
            ConnectedClient client,
            ConnectedClientStore sut)
        {
            sut.Add(client);
            var result = sut.Find(client.ClientId);
            Assert.NotNull(result);
            Assert.Equal(client, result);
            Assert.Equal(updateDetector, GetPrivateField(client, "_updateDetector"));
        }

        [Theory, AutoMoqData]
        public void ShouldThrow_WhenAlreadyAdded(
            ConnectedClient client,
            ConnectedClientStore sut)
        {
            sut.Add(client);
            Assert.Throws<ClientAlreadyConnectedException>(
                () => sut.Add(new ConnectedClient(
                    client.ClientId,
                    Create<IConnection>(),
                    Create<string>(),
                    Create<IUpdateDetector>())));
        }

        [Theory, AutoMoqData]
        public void ShouldNotFind_WhenNotAdded(ConnectedClientStore sut)
        {
            Assert.Null(sut.Find(Create<string>()));
        }

        [Theory, AutoMoqData]
        public void ShouldFind_WhenAdded(
            ConnectedClient client,
            ConnectedClientStore sut)
        {
            sut.Add(client);
            var found = sut.Find(client.ClientId);

            Assert.Equal(client, found);
        }

        [Theory, AutoMoqData]
        public void ShouldMarkForUpdate_WhenSuccessfullyAdded(
            [Frozen]Mock<IUpdateDetector> updateDetector,
            ConnectedClient client,
            ConnectedClientStore sut)
        {
            sut.Add(client);
            updateDetector.Verify(x => x.MarkForUpdate(client.Group), Times.Once);

            // Unsuccessful add.
            try
            {
                sut.Add(client);
            }
            catch (ClientAlreadyConnectedException) { }

            updateDetector.Verify(x => x.MarkForUpdate(client.Group), Times.Once);
        }

        [Theory, AutoMoqData]
        public void ShouldMarkForUpdate_WhenSuccessfullyRemoved(
            [Frozen]Mock<IUpdateDetector> updateDetector,
            ConnectedClient client,
            ConnectedClientStore sut)
        {
            sut.Add(client);
            updateDetector.Verify(x => x.MarkForUpdate(client.Group), Times.Once);

            sut.Remove(client.ClientId);
            updateDetector.Verify(x => x.MarkForUpdate(client.Group), Times.Exactly(2));
        }

        [Theory, AutoMoqData]
        public void ShouldFindAllClientsInGroups(ConnectedClientStore sut)
        {
            var groups = Fixture.CreateMany<string>(3).ToList();

            var clients = Fixture.CreateMany<ConnectedClient>(6).ToList();
            clients[0].Group = groups[0];
            clients[1].Group = groups[1];
            clients[2].Group = groups[2];
            clients[3].Group = groups[0];
            clients[4].Group = groups[1];
            clients[5].Group = groups[2];

            foreach (var client in clients)
            {
                sut.Add(client);
            }

            var result = sut.FindInGroups(new[] { groups[0], groups[2] });
            Assert.Equal(4, result.Count());
            Assert.Contains(clients[0], result);
            Assert.DoesNotContain(clients[1], result);
            Assert.Contains(clients[2], result);
            Assert.Contains(clients[3], result);
            Assert.DoesNotContain(clients[4], result);
            Assert.Contains(clients[5], result);
        }

        [Theory, AutoMoqData]
        public void ShouldNotThrow_WhenRemovingNonExistingClient(ConnectedClientStore sut)
        {
            sut.Remove(Create<string>());
        }

        [Theory, AutoMoqData]
        public void ShouldRemoveClient(
            ConnectedClient client, ConnectedClientStore sut)
        {
            sut.Add(client);
            Assert.Equal(client, sut.Find(client.ClientId));

            sut.Remove(client.ClientId);
            Assert.Null(sut.Find(client.ClientId));
        }

        [Theory, AutoMoqData]
        public async Task ConcurrencyTest(string clientId, ConnectedClientStore sut)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                tasks.Add(new Task(() => sut.Add(ClientWithId(clientId))));
                tasks.Add(new Task(() => sut.Remove(clientId)));
                tasks.Add(new Task(() => sut.Find(clientId)));
            }

            Parallel.ForEach(tasks, task => task.Start());
            try { await Task.WhenAll(tasks); } catch (ClientAlreadyConnectedException) { }
            try { sut.Add(ClientWithId(clientId)); } catch (ClientAlreadyConnectedException) { }
            Assert.NotNull(sut.Find(clientId));
        }

        private ConnectedClient ClientWithId(string clientId)
        {
            return new ConnectedClient(
                clientId,
                Create<IConnection>(),
                Create<string>(),
                Create<IUpdateDetector>());
        }
    }
}
