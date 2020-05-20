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
        public void ShouldBe1MessageInAssembly()
        {
            Assert.Equal(1, typeof(Announce).Assembly.GetTypes().Count(
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
    }
}
