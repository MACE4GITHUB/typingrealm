using System.Linq;
using System.Reflection;
using TypingRealm.Messaging.Messages;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
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

        public class TestAbstractMessage : Message
        {
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
            AssertSerializable<Announce>();

            var sut = new Announce(message);
            Assert.Equal(message, sut.Message);
        }

        [Theory, AutoMoqData]
        public void DisconnectedMessage(string reason)
        {
            AssertSerializable<Disconnected>();

            var sut = new Disconnected(reason);
            Assert.Equal(reason, sut.Reason);
        }

        [Fact]
        public void DisconnectMessage()
        {
            AssertSerializable<Disconnect>();
        }

        [Theory, AutoMoqData]
        public void ConnectMessage(
            string clientId,
            string group)
        {
            Assert.NotNull(Connect.DefaultGroup);
            AssertSerializable<Connect>();

            var sut = new Connect(clientId);
            Assert.Equal(clientId, sut.ClientId);
            Assert.Equal(Connect.DefaultGroup, sut.Group);

            sut = new Connect(clientId, group);
            Assert.Equal(clientId, sut.ClientId);
            Assert.Equal(group, sut.Group);

            sut = new Connect();
            Assert.Equal(Connect.DefaultGroup, sut.Group);
        }

        [Theory, AutoMoqData]
        public void BroadcastMessage(string senderId)
        {
            AssertSerializable<TestBroadcastMessage>();

            var sut = new TestBroadcastMessage(senderId);
            Assert.Equal(senderId, sut.SenderId);
        }

        [Theory, AutoMoqData]
        public void AbstractMessage(string messageId)
        {
            AssertSerializable<TestAbstractMessage>();

            var sut = new TestAbstractMessage();
            Assert.Null(sut.MessageId);

            sut.MessageId = messageId;
            Assert.Equal(messageId, sut.MessageId);

            sut.MessageId = null;
            Assert.Null(sut.MessageId);
        }

        [Theory, AutoMoqData]
        public void AcknowledgeReceived(AcknowledgeReceived sut)
        {
            AssertSerializable<AcknowledgeReceived>();

            Assert.IsAssignableFrom<Message>(sut);
        }
    }
}
