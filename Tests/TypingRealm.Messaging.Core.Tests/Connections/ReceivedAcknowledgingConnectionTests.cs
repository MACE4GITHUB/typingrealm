using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections;

public class ReceivedAcknowledgingConnectionTests : TestsBase
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

    [Theory]
    [InlineAutoMoqData(AcknowledgementType.None)]
    [InlineAutoMoqData(AcknowledgementType.Handled)]
    public async Task ShouldNotSendAcknowledgeReceived_WhenAcknowledgementTypeIsNotReceived(
        AcknowledgementType acknowledgementType,
        MessageWithMetadata message,
        [Frozen] Mock<IConnection> connection,
        ReceivedAcknowledgingConnection sut)
    {
        connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(message);

        message.Metadata!.AcknowledgementType = acknowledgementType;

        await sut.ReceiveAsync(Cts.Token);

        connection.Verify(x => x.SendAsync(It.IsAny<object>(), Cts.Token), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task ShouldSendAcknowledgeReceived_WhenMessageHasId_AndAcknowledgementTypeIsReceived(
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
                y => (y.Message is AcknowledgeReceived) != null
                && ((AcknowledgeReceived)y.Message).MessageId == message.Metadata.MessageId
                && y.Metadata!.MessageId == message.Metadata.MessageId),
            Cts.Token));
    }

    [Theory, AutoMoqData]
    public async Task ShouldNotSendAcknowledgeReceived_WhenMessageHasId_AndAcknowledgementTypeIsReceived_ButMessageIsAcknowledgement(
        MessageWithMetadata message,
        [Frozen] Mock<IConnection> connection,
        ReceivedAcknowledgingConnection sut)
    {
        connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(message);

        message.Metadata!.AcknowledgementType = AcknowledgementType.Received;

        message.Message = new AcknowledgeReceived();
        await sut.ReceiveAsync(Cts.Token);

        message.Message = new AcknowledgeHandled();
        await sut.ReceiveAsync(Cts.Token);

        connection.Verify(x => x.SendAsync(It.IsAny<object>(), Cts.Token), Times.Never);
    }
}
