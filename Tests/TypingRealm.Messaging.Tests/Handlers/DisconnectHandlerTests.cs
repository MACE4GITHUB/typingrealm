using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Handlers;
using TypingRealm.Messaging.Messages;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handlers;

public class DisconnectHandlerTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task ShouldRemoveClientFromStore(
        [Frozen] Mock<IConnectedClientStore> store,
        ConnectedClient client,
        DisconnectHandler sut)
    {
        await sut.HandleAsync(client, Create<Disconnect>(), Cts.Token);

        store.Verify(x => x.Remove(client.ClientId));
    }

    [Theory, AutoMoqData]
    public async Task ShouldSendDisconnectedMessage(
        [Frozen] Mock<IConnection> connection,
        ConnectedClient client,
        DisconnectHandler sut)
    {
        await sut.HandleAsync(client, Create<Disconnect>(), Cts.Token);

        connection.Verify(x => x.SendAsync(It.IsAny<Disconnected>(), Cts.Token));
    }
}
