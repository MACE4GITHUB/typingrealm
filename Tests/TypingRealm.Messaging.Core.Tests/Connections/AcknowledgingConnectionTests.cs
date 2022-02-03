using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections;

public class AcknowledgingConnectionTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task ShouldSendAsUsual(
        object message,
        [Frozen] Mock<IConnection> connection,
        ReceivedAcknowledgingConnection sut)
    {
        await sut.SendAsync(message, Cts.Token);

        connection.Verify(x => x.SendAsync(message, Cts.Token));
    }

    [Theory, AutoMoqData]
    public async Task ShouldNotSendAcknowledgeReceived_WhenMessageDoesNotHaveMetadata(
        [Frozen] Mock<IConnection> connection,
        ReceivedAcknowledgingConnection sut)
    {
        await sut.ReceiveAsync(Cts.Token);

        connection.Verify(x => x.SendAsync(It.IsAny<object>(), Cts.Token), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task ShouldNotSendAcknowledgeReceived_WhenMessageIdIsNotSet(
        MessageWithMetadata message,
        [Frozen] Mock<IConnection> connection,
        ReceivedAcknowledgingConnection sut)
    {
        connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(message);

        message.Metadata!.MessageId = null;
        message.Metadata.AcknowledgementType = AcknowledgementType.Received;

        await sut.ReceiveAsync(Cts.Token);

        connection.Verify(x => x.SendAsync(It.IsAny<object>(), Cts.Token), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task ShouldNotSendAcknowledgeReceived_WhenRequireAcknowledgementIsFalse(
        MessageWithMetadata message,
        [Frozen] Mock<IConnection> connection,
        ReceivedAcknowledgingConnection sut)
    {
        connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(message);

        message.Metadata!.AcknowledgementType = AcknowledgementType.None;

        await sut.ReceiveAsync(Cts.Token);

        connection.Verify(x => x.SendAsync(It.IsAny<object>(), Cts.Token), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task ShouldSendAcknowledgeReceived_WhenMessageHasId_AndRequireAcknowledgementIsTrue(
        MessageWithMetadata message,
        [Frozen] Mock<IConnection> connection,
        ReceivedAcknowledgingConnection sut)
    {
        connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(message);

        message.Metadata!.AcknowledgementType = AcknowledgementType.Received;

        await sut.ReceiveAsync(Cts.Token);

        connection.Verify(x => x.SendAsync(
            It.Is<MessageWithMetadata>(
                y => (y.Message as AcknowledgeReceived) != null
                && ((AcknowledgeReceived)y.Message).MessageId == message.Metadata.MessageId
                && y.Metadata!.MessageId == message.Metadata.MessageId),
            Cts.Token));
    }
}
