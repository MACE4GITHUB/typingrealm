using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageWithMetadataTests : TestsBase
{
    [Theory, AutoMoqData]
    public void ClientToServer_ShouldBeSerializable(
        string message,
        ClientToServerMessageMetadata metadata)
    {
        AssertSerializable<MessageWithMetadata>();

        var sut = new MessageWithMetadata(message, metadata);
        Assert.Equal(message, sut.Message);
        Assert.Equal(metadata, sut.Metadata);
    }

    [Theory, AutoMoqData]
    public void ShouldBeSerializable(
        string message,
        MessageMetadata metadata)
    {
        AssertSerializable<MessageWithMetadata>();

        var sut = new MessageWithMetadata(message, metadata);
        Assert.Equal(message, sut.Message);
        Assert.Equal(metadata, sut.Metadata);
    }
}
