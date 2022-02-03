using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageMetadataTests : TestsBase
{
    [Fact]
    public void ShouldCreateEmpty()
    {
        var sut = MessageMetadata.CreateEmpty();

        Assert.Null(sut.MessageId);
    }

    [Fact]
    public void ShouldBeSerializable()
    {
        AssertSerializable<MessageMetadata>();
    }
}
