using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class ServerToClientMessageWithMetadataTests : TestsBase
{
    [Theory, AutoMoqData]
    public void ShouldBeSerializable(
        string message,
        ServerToClientMessageMetadata metadata)
    {
        AssertSerializable<ServerToClientMessageMetadata>();

        var sut = new MessageWithMetadata(message, metadata);
        Assert.Equal(message, sut.Message);
        Assert.Equal(metadata, sut.Metadata);
    }
}
