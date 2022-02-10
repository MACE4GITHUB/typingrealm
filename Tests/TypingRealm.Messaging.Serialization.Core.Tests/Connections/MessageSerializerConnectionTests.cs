using System;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using TypingRealm.Messaging.Serialization.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Connections;

public class MessageSerializerConnectionTests : TestsBase
{
    private readonly Mock<IMessageMetadataFactory> _metadataFactory;
    private readonly Mock<IMessageTypeCache> _messageTypeCache;
    private readonly Mock<IMessageSerializer> _messageSerializer;
    private readonly Mock<IConnection> _connection;
    private readonly MessageSerializerConnection _sut;

    public MessageSerializerConnectionTests()
    {
        _metadataFactory = Freeze<IMessageMetadataFactory>();
        _messageTypeCache = Freeze<IMessageTypeCache>();
        _messageSerializer = Freeze<IMessageSerializer>();
        _connection = Freeze<IConnection>();
        _sut = Fixture.Create<MessageSerializerConnection>();
    }

    [Fact]
    public async Task ReceiveAsync_ShouldThrow_WhenReceivedMessageIsNotMessageData()
    {
        _connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(Fixture.Create<object>());

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.ReceiveAsync(Cts.Token));
    }

    [Theory, AutoMoqData]
    public async Task ReceiveAsync_ShouldHaveDefaultMetatada_WhenMessageHasNoMetadata(
        MessageData messageData, Type type, object deserialized)
    {
        messageData.Metadata = null;

        _connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(messageData);

        _messageTypeCache.Setup(x => x.GetTypeById(messageData.TypeId))
            .Returns(type);

        _messageSerializer.Setup(x => x.Deserialize(messageData.Data, type))
            .Returns(deserialized);

        var result = await _sut.ReceiveAsync(Cts.Token);

        Assert.IsType<MessageWithMetadata>(result);
        var mwm = (MessageWithMetadata)result;
        Assert.Equal(deserialized, mwm.Message);
        Assert.NotNull(mwm.Metadata);
        Assert.Null(mwm.Metadata!.MessageId);
        Assert.Equal(AcknowledgementType.Handled, mwm.Metadata.AcknowledgementType);
    }

    [Theory, AutoMoqData]
    public async Task ReceiveAsync_ShouldHaveMetatada_WhenMessageHasMetadata(
        MessageData messageData, Type type, object deserialized)
    {
        _connection.Setup(x => x.ReceiveAsync(Cts.Token))
            .ReturnsAsync(messageData);

        _messageTypeCache.Setup(x => x.GetTypeById(messageData.TypeId))
            .Returns(type);

        _messageSerializer.Setup(x => x.Deserialize(messageData.Data, type))
            .Returns(deserialized);

        var result = await _sut.ReceiveAsync(Cts.Token);

        Assert.IsType<MessageWithMetadata>(result);
        var mwm = (MessageWithMetadata)result;
        Assert.Equal(deserialized, mwm.Message);
        Assert.NotNull(mwm.Metadata);
        Assert.Equal(messageData.Metadata, mwm.Metadata);
    }

    [Theory, AutoMoqData]
    public async Task SendAsync_ShouldHaveDefaultMetadata_WhenMessageIsNotWithMetadata(
        object message, string serialized, string typeId, MessageMetadata metadata)
    {
        _messageSerializer.Setup(x => x.Serialize(message))
            .Returns(serialized);

        _messageTypeCache.Setup(x => x.GetTypeId(message.GetType()))
            .Returns(typeId);

        _metadataFactory.Setup(x => x.CreateFor(message))
            .Returns(metadata);

        await _sut.SendAsync(message, Cts.Token);

        _connection.Verify(x => x.SendAsync(It.Is<MessageData>(msg =>
            msg.Data == serialized
            && msg.TypeId == typeId
            && msg.Metadata == metadata), Cts.Token));
    }

    [Theory, AutoMoqData]
    public async Task SendAsync_ShouldHaveDefaultMetadata_WhenMessageMetadataIsNull(
        MessageWithMetadata message, string serialized, string typeId, MessageMetadata metadata)
    {
        message.Metadata = null;

        _messageSerializer.Setup(x => x.Serialize(message.Message))
            .Returns(serialized);

        _messageTypeCache.Setup(x => x.GetTypeId(message.Message.GetType()))
            .Returns(typeId);

        _metadataFactory.Setup(x => x.CreateFor(message.Message))
            .Returns(metadata);

        await _sut.SendAsync(message, Cts.Token);

        _connection.Verify(x => x.SendAsync(It.Is<MessageData>(msg =>
            msg.Data == serialized
            && msg.TypeId == typeId
            && msg.Metadata == metadata), Cts.Token));
    }

    [Theory, AutoMoqData]
    public async Task SendAsync_ShouldHaveMetadata_WhenMessageMetadataIsSet(
        MessageWithMetadata message, string serialized, string typeId)
    {
        _messageSerializer.Setup(x => x.Serialize(message.Message))
            .Returns(serialized);

        _messageTypeCache.Setup(x => x.GetTypeId(message.Message.GetType()))
            .Returns(typeId);

        await _sut.SendAsync(message, Cts.Token);

        _connection.Verify(x => x.SendAsync(It.Is<MessageData>(msg =>
            msg.Data == serialized
            && msg.TypeId == typeId
            && msg.Metadata == message.Metadata), Cts.Token));
    }
}
