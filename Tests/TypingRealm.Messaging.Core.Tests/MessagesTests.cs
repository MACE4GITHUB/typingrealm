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
        public void ShouldBe2MessagesInAssembly()
        {
            Assert.Equal(2, typeof(Announce).Assembly.GetTypes().Count(
                t => t.GetCustomAttribute<MessageAttribute>() != null));
        }

        [Fact]
        public void AnnounceMessage()
        {
            AssertSerializable<Announce>();

            var sut = new Announce
            {
                Message = "message"
            };
            Assert.Equal("message", sut.Message);

            sut = new Announce("message");
            Assert.Equal("message", sut.Message);
        }

        [Fact]
        public void DisconnectedMessage()
        {
            AssertSerializable<Disconnected>();

            var sut = new Disconnected
            {
                Reason = "reason"
            };
            Assert.Equal("reason", sut.Reason);

            sut = new Disconnected("reason");
            Assert.Equal("reason", sut.Reason);
        }
    }
}
