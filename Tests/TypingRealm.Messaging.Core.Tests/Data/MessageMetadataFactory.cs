using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageMetadataFactory : TestsBase
{
    [Theory, AutoMoqData]
    public void ShouldHaveHandledAcknowledgementByDefault(
        object message,
        Messaging.MessageMetadataFactory sut)
    {
        var result = sut.CreateFor(message);
        Assert.Equal(AcknowledgementType.Handled, result.AcknowledgementType);
    }

    [Theory, AutoMoqData]
    public void ShouldGetMessageIdFromFactory(
        [Frozen] Mock<IMessageIdFactory> messageIdFactory,
        string messageId,
        object message,
        Messaging.MessageMetadataFactory sut)
    {
        messageIdFactory.Setup(x => x.CreateMessageId())
            .Returns(messageId);

        var result = sut.CreateFor(message);

        Assert.Equal(messageId, result.MessageId);
    }

    [Theory, AutoMoqData]
    public void ShouldHaveDefaultSendUpdateToFalse(
        Messaging.MessageMetadataFactory sut)
    {
        var result = sut.CreateFor(Create<object>());
        Assert.False(result.SendUpdate);
    }

    [Message(sendUpdate: false)]
    private class TestMessageNoUpdate { }

    [Message(sendUpdate: true)]
    private class TestMessageWithUpdate { }

    [Theory, AutoMoqData]
    public void ShouldLoadSendUpdateFromTheAttribute(
        Messaging.MessageMetadataFactory sut)
    {
        var result = sut.CreateFor(new TestMessageNoUpdate());
        Assert.False(result.SendUpdate);

        result = sut.CreateFor(new TestMessageWithUpdate());
        Assert.True(result.SendUpdate);
    }
}
