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
    public class ConnectedClientStoreExtensionsTests : MessagingTestsBase
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

        [Theory, SingleGroupData]
        public async Task SendAsync_ShouldSendMessageToAllClientsThatAreInSpecifiedGroup(ConnectedClientStore sut)
        {
            var connections = Fixture.CreateMany<IConnection>(3).ToList();

            var clients = connections
                .Select(connection => CreateSingleGroupClient(connection))
                .ToList();

            foreach (var client in clients) { sut.Add(client); }
            clients[1].Group = clients[0].Group;

            Mock.Get(connections[0]).Setup(x => x.SendAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Callback(() => { });

            var message = Create<TestMessage>();
            await sut.SendAsync(message, clients[0].Group, Cts.Token);

            Mock.Get(connections[0]).Verify(x => x.SendAsync(message, Cts.Token));
            Mock.Get(connections[1]).Verify(x => x.SendAsync(message, Cts.Token));
            Mock.Get(connections[2]).Verify(x => x.SendAsync(message, Cts.Token), Times.Never);
        }

        [Theory, MultiGroupData]
        public async Task SendAsync_ShouldSendMessageToAllClientsThatAreInSpecifiedGroups(ConnectedClientStore sut)
        {
            var connections = Fixture.CreateMany<IConnection>(3).ToList();

            var clients = connections
                .Select(connection => CreateMultiGroupClient(connection: connection))
                .ToList();

            foreach (var client in clients) { sut.Add(client); }
            clients[1].AddToGroup(clients[0].Groups.ToList()[0]);

            Mock.Get(connections[0]).Setup(x => x.SendAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Callback(() => { });

            var message = Create<TestMessage>();
            await sut.SendAsync(message, clients[0].Groups.ToList()[0], Cts.Token);

            Mock.Get(connections[0]).Verify(x => x.SendAsync(message, Cts.Token));
            Mock.Get(connections[1]).Verify(x => x.SendAsync(message, Cts.Token));
            Mock.Get(connections[2]).Verify(x => x.SendAsync(message, Cts.Token), Times.Never);
        }
    }
}
