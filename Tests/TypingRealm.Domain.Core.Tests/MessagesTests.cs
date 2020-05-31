using System.Collections.Generic;
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
            Assert.Equal(3, typeof(Join).Assembly.GetTypes().Count(
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

        [Theory, AutoMoqData]
        public void UpdateMessage(
            string locationId,
            IEnumerable<string> visiblePlayers)
        {
            AssertSerializable<Update>();

            var sut = new Update
            {
                LocationId = locationId,
                VisiblePlayers = visiblePlayers
            };
            Assert.Equal(locationId, sut.LocationId);
            Assert.Equal(visiblePlayers, sut.VisiblePlayers);

            sut = new Update(locationId, visiblePlayers);
            Assert.Equal(locationId, sut.LocationId);
            Assert.Equal(visiblePlayers, sut.VisiblePlayers);
        }
    }
}
