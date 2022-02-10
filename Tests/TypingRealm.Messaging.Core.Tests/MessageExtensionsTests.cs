using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageExtensionsTests
{
    [Theory, AutoMoqData]
    public void GetMetadataOrEmpty_ShouldGetDefaultMetadata_WhenObjectIsNotWithMetadataMessage(
        object message)
    {
        var metadata = message.GetMetadataOrEmpty();

        Assert.Equal(AcknowledgementType.Handled, metadata.AcknowledgementType);
        Assert.Null(metadata.MessageId);
    }

    [Theory, AutoMoqData]
    public void GetMetadataOrEmpty_ShouldGetDefaultMetadata_WhenObjectDoesNotHaveMetadata(
        MessageWithMetadata message)
    {
        message.Metadata = null!;

        var metadata = message.GetMetadataOrEmpty();

        Assert.Equal(AcknowledgementType.Handled, metadata.AcknowledgementType);
        Assert.Null(metadata.MessageId);
    }

    [Theory, AutoMoqData]
    public void GetMetadataOrEmpty_ShouldGetMetadata_WhenObjectHasMetadata(
        MessageMetadata metadata,
        MessageWithMetadata message)
    {
        message.Metadata = metadata;

        var result = message.GetMetadataOrEmpty();

        Assert.Equal(metadata, result);
    }
}
