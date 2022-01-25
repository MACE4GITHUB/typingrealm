using AutoFixture.Xunit2;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connecting;

public class ClientAlreadyConnectedExceptionTests
{
    [Theory, AutoMoqData]
    public void ShouldContainClientIdInMessage(
        [Frozen] ConnectedClient client,
        ClientAlreadyConnectedException sut)
    {
        Assert.Contains(client.ClientId, sut.Message);
    }

    [Theory, AutoMoqData]
    public void ShouldContainAlreadyConnectedMessage(ClientAlreadyConnectedException sut)
    {
        Assert.Contains("already connected", sut.Message);
    }
}
