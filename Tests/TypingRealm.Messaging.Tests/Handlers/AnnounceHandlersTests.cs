using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Handling.Handlers;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handlers
{
    public class AnnounceHandlersTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldBroadcastToGroup(
            [Frozen] Mock<IConnectedClientStore> connectedClients,
            ConnectedClient sender,
            Announce message,
            AnnounceHandler sut)
        {
            var connections = Fixture.CreateMany<Mock<IConnection>>().ToList();
            var clients = connections.Select(x => FromConnection(x.Object)).ToList();

            connectedClients
                .Setup(x => x.FindInGroups(It.Is<IEnumerable<string>>(
                    groups => groups.Count() == 1
                        && groups.Contains(sender.Group))))
                .Returns(clients);

            await sut.HandleAsync(sender, message, default);

            foreach (var connection in connections)
            {
                connection.Verify(x => x.SendAsync(
                    It.Is<Announce>(m => m.Message.Contains(message.Message)),
                    default));
            }
        }

        private ConnectedClient FromConnection(IConnection connection)
        {
            return new ConnectedClient(
                Create<string>(),
                connection,
                Create<string>(),
                Create<IUpdateDetector>());
        }
    }
}
