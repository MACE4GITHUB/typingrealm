using System.Linq;
using System.Reflection;
using TypingRealm.Messaging.Messages;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class MessagesTests : TestsBase
    {
        [Fact]
        public void ShouldHaveTestsForAllMessages()
        {
            Assert.Equal(4, typeof(Announce).Assembly.GetTypes().Count(
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
    }
}
