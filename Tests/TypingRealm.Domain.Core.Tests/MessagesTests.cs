using System.Linq;
using System.Reflection;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Domain.Tests
{
    public class MessagesTests : TestsBase
    {
        [Fact]
        public void ShouldHaveTestsForAllMessages()
        {
            Assert.Equal(2, typeof(Join).Assembly.GetTypes().Count(
                t => t.GetCustomAttribute<MessageAttribute>() != null));
        }

        [Fact]
        public void JoinMessage()
        {
            AssertSerializable<Join>();

            var sut = new Join
            {
                Name = "name"
            };
            Assert.Equal("name", sut.Name);

            sut = new Join("name");
            Assert.Equal("name", sut.Name);
        }

        [Fact]
        public void MoveToMessage()
        {
            AssertSerializable<MoveTo>();

            var sut = new MoveTo
            {
                LocationId = "locationId"
            };
            Assert.Equal("locationId", sut.LocationId);

            sut = new MoveTo("locationId");
            Assert.Equal("locationId", sut.LocationId);
        }
    }
}
