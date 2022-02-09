using System.Linq;
using System.Reflection;
using TypingRealm.Messaging.Messages;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessagesTests : TestsBase
{
    public class TestBroadcastMessage : BroadcastMessage
    {
        public TestBroadcastMessage() : base()
        {
        }

        public TestBroadcastMessage(string senderId) : base(senderId)
        {
        }
    }

    [Fact]
    public void ShouldHaveTestsForAllMessages()
    {
        Assert.Equal(7, typeof(Announce).Assembly.GetTypes().Count(
            t => t.GetCustomAttribute<MessageAttribute>() != null));
    }

    [Theory, AutoMoqData]
    public void AnnounceMessage(string message)
    {
        AssertSerializableMessage<Announce>();

        var sut = new Announce(message);
        Assert.Equal(message, sut.Message);
    }

    [Theory, AutoMoqData]
    public void BroadcastMessage(string senderId)
    {
        AssertSerializableMessage<TestBroadcastMessage>();

        var sut = new TestBroadcastMessage(senderId);
        Assert.Equal(senderId, sut.SenderId);
    }

    [Theory, AutoMoqData]
    public void ConnectMessage(
        string clientId,
        string group)
    {
        Assert.NotNull(Connect.DefaultGroup);
        AssertSerializableMessage<Connect>();

        var sut = new Connect(clientId);
        Assert.Equal(clientId, sut.ClientId);
        Assert.Equal(Connect.DefaultGroup, sut.Group);

        sut = new Connect(clientId, group);
        Assert.Equal(clientId, sut.ClientId);
        Assert.Equal(group, sut.Group);

        sut = new Connect();
        Assert.Equal(Connect.DefaultGroup, sut.Group);
    }

    [Fact]
    public void DisconnectMessage()
    {
        AssertSerializableMessage<Disconnect>();
    }

    [Theory, AutoMoqData]
    public void DisconnectedMessage(string reason)
    {
        AssertSerializableMessage<Disconnected>();

        var sut = new Disconnected(reason);
        Assert.Equal(reason, sut.Reason);
    }

    [Theory, AutoMoqData]
    public void AcknowledgeReceived(string messageId)
    {
        AssertSerializableMessage<AcknowledgeReceived>();

        var sut = new AcknowledgeReceived(messageId);
        Assert.Equal(messageId, sut.MessageId);
    }

    [Theory, AutoMoqData]
    public void AcknowledgeHandled(string messageId)
    {
        AssertSerializableMessage<AcknowledgeHandled>();

        var sut = new AcknowledgeHandled(messageId);
        Assert.Equal(messageId, sut.MessageId);
    }
}
