using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections;

public class ConnectionExtensionsTests : TestsBase
{
    [Theory, AutoMoqData]
    public void WithReceiveAcknowledgement_ShouldWrapConnection(
        IConnection connection)
    {
        var sut = connection.WithReceiveAcknowledgement();

        Assert.IsType<ReceivedAcknowledgingConnection>(sut);
        Assert.Equal(connection, GetPrivateField(sut, "_connection"));
    }
}
