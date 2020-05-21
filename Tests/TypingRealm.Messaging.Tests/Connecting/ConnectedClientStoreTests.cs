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
            string clientId, IConnection connection, string group,
            ConnectedClientStore sut)
        {
            var client = sut.Add(clientId, connection, group);
            Assert.NotNull(client);
            Assert.Equal(clientId, client!.ClientId);
            Assert.Equal(connection, client.Connection);
            Assert.Equal(group, client.Group);
            Assert.Equal(updateDetector, GetPrivateField(client, "_updateDetector"));
        }

        [Theory, AutoMoqData]
        public void ShouldNotAddClient_WhenAlreadyAdded(string clientId, ConnectedClientStore sut)
        {
            sut.Add(clientId, Create<IConnection>(), Create<string>());
            Assert.Null(sut.Add(clientId, Create<IConnection>(), Create<string>()));
        }

        [Theory, AutoMoqData]
        public void ShouldNotFind_WhenNotAdded(ConnectedClientStore sut)
        {
            Assert.Null(sut.Find(Create<string>()));
        }

        [Theory, AutoMoqData]
        public void ShouldFind_WhenAdded(string clientId, ConnectedClientStore sut)
        {
            var created = sut.Add(clientId, Create<IConnection>(), Create<string>());
            var found = sut.Find(clientId);

            Assert.Equal(created, found);
        }

        [Theory, AutoMoqData]
        public void ShouldMarkForUpdate_WhenSuccessfullyAdded(
            [Frozen]Mock<IUpdateDetector> updateDetector,
            string clientId,
            string group,
            ConnectedClientStore sut)
        {
            sut.Add(clientId, Create<IConnection>(), group);
            updateDetector.Verify(x => x.MarkForUpdate(group), Times.Once);

            sut.Add(clientId, Create<IConnection>(), group); // Unsuccessful add.
            updateDetector.Verify(x => x.MarkForUpdate(group), Times.Once);
        }

        [Theory, AutoMoqData]
        public void ShouldFindAllClientsInGroups(ConnectedClientStore sut)
        {
            var ids = Fixture.CreateMany<string>(6).ToList();
            var groups = Fixture.CreateMany<string>(3).ToList();

            var c1 = sut.Add(ids[0], Create<IConnection>(), groups[0]);
            var c2 = sut.Add(ids[1], Create<IConnection>(), groups[1]);
            var c3 = sut.Add(ids[2], Create<IConnection>(), groups[2]);
            var c4 = sut.Add(ids[3], Create<IConnection>(), groups[0]);
            var c5 = sut.Add(ids[4], Create<IConnection>(), groups[1]);
            var c6 = sut.Add(ids[5], Create<IConnection>(), groups[2]);

            var clients = sut.FindInGroups(new[] { groups[0], groups[2] });
            Assert.Equal(4, clients.Count());
            Assert.Contains(c1, clients);
            Assert.DoesNotContain(c2, clients);
            Assert.Contains(c3, clients);
            Assert.Contains(c4, clients);
            Assert.DoesNotContain(c5, clients);
            Assert.Contains(c6, clients);
        }

        [Theory, AutoMoqData]
        public void ShouldNotThrow_WhenRemovingNonExistingClient(ConnectedClientStore sut)
        {
            sut.Remove(Create<string>());
        }

        [Theory, AutoMoqData]
        public void ShouldRemoveClient(string clientId, ConnectedClientStore sut)
        {
            var client = sut.Add(clientId, Create<IConnection>(), Create<string>());
            Assert.Equal(client, sut.Find(clientId));

            sut.Remove(clientId);
            Assert.Null(sut.Find(clientId));
        }

        [Theory, AutoMoqData]
        public async Task ConcurrencyTest(string clientId, ConnectedClientStore sut)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                tasks.Add(new Task(() => sut.Add(clientId, Create<IConnection>(), Create<string>())));
                tasks.Add(new Task(() => sut.Remove(clientId)));
                tasks.Add(new Task(() => sut.Find(clientId)));
            }

            Parallel.ForEach(tasks, task => task.Start());
            await Task.WhenAll(tasks);
            sut.Add(clientId, Create<IConnection>(), Create<string>());
            Assert.NotNull(sut.Find(clientId));
        }
    }
}
