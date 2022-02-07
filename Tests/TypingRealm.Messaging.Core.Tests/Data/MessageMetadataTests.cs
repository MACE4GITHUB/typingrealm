using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageMetadataTests : TestsBase
{
    private const AcknowledgementType DefaultAcknowledgementType
        = AcknowledgementType.Handled;

    [Fact]
    public void ShouldCreateEmpty()
    {
        var sut = MessageMetadata.CreateEmpty();

        Assert.Null(sut.MessageId);
        Assert.Equal(DefaultAcknowledgementType, sut.AcknowledgementType);
        Assert.Null(sut.ResponseMessageTypeId);
    }

    [Fact]
    public void ShouldBeSerializable()
    {
        AssertSerializable<MessageMetadata>();
    }

    [Fact]
    public void ShouldHaveHandledDefaultAcknowledgementType()
    {
        var sut = MessageMetadata.CreateEmpty();
        Assert.Equal(AcknowledgementType.Handled, sut.AcknowledgementType);

        sut = new MessageMetadata();
        Assert.Equal(AcknowledgementType.Handled, sut.AcknowledgementType);
    }
}
