using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Handlers;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Tests.SpecimenBuilders;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handlers
{
    public class AnnounceHandlerTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldBroadcastMessageToGroup(
            [Frozen] Mock<IConnectedClientStore> connectedClients,
            ConnectedClient sender,
            Announce announce,
            CancellationToken ct,
            AnnounceHandler sut)
        {
            var connections = Fixture.CreateMany<IConnection>().ToList();
            var clients = connections
                .Select(connection => Create<ConnectedClient>(
                    new ClientWithConnectionSpecimenBuilder(connection)))
                .ToList();

            connectedClients
                .Setup(x => x.FindInGroups(It.Is<IEnumerable<string>>(
                    groups => groups.Count() == 1
                        && groups.Contains(sender.Group))))
                .Returns(clients);

            await sut.HandleAsync(sender, announce, ct);

            foreach (var connection in connections)
            {
                Mock.Get(connection).Verify(x => x.SendAsync(
                    It.Is<Announce>(m => m.Message.Contains(announce.Message)),
                    ct));
            }
        }
    }
}
