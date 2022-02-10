﻿using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageMetadataFactory
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
}