using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageWithMetadataTests : TestsBase
{
    [Theory, AutoMoqData]
    public void ShouldBeSerializable(
        string message,
        ClientToServerMessageMetadata metadata)
    {
        AssertSerializable<MessageWithMetadata>();

        var sut = new MessageWithMetadata(message, metadata);
        Assert.Equal(message, sut.Message);
        Assert.Equal(metadata, sut.Metadata);
    }
}
