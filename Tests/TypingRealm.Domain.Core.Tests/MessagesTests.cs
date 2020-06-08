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
            Assert.Equal(9, typeof(Join).Assembly.GetTypes().Count(
                t => t.GetCustomAttribute<MessageAttribute>() != null));
        }

        [Fact]
        public void JoinMessage()
        {
            AssertSerializable<Join>();

            var sut = new Join
            {
                PlayerId = "name"
            };
            Assert.Equal("name", sut.PlayerId);

            sut = new Join("name");
            Assert.Equal("name", sut.PlayerId);
        }

        [Fact]
        public void MoveToMessage()
        {
            AssertSerializable<MoveToLocation>();

            var sut = new MoveToLocation
            {
                LocationId = "locationId"
            };
            Assert.Equal("locationId", sut.LocationId);

            sut = new MoveToLocation("locationId");
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

        [Theory, AutoMoqData]
        public void CombatUpdateMessage(string enemyId)
        {
            AssertSerializable<CombatUpdate>();

            var sut = new CombatUpdate
            {
                EnemyId = enemyId
            };
            Assert.Equal(enemyId, sut.EnemyId);

            sut = new CombatUpdate(enemyId);
            Assert.Equal(enemyId, sut.EnemyId);
        }

        [Theory, AutoMoqData]
        public void AttackMessage(string playerId)
        {
            AssertSerializable<Attack>();

            var sut = new Attack
            {
                PlayerId = playerId
            };
            Assert.Equal(playerId, sut.PlayerId);

            sut = new Attack(playerId);
            Assert.Equal(playerId, sut.PlayerId);
        }

        [Fact]
        public void SurrenderMessage()
        {
            AssertSerializable<Surrender>();
        }

        [Fact]
        public void TurnAroundMessage()
        {
            AssertSerializable<TurnAround>();
        }

        [Theory, AutoMoqData]
        public void EnterRoadMessage(string roadId)
        {
            AssertSerializable<EnterRoad>();

            var sut = new EnterRoad
            {
                RoadId = roadId
            };
            Assert.Equal(roadId, sut.RoadId);

            sut = new EnterRoad(roadId);
            Assert.Equal(roadId, sut.RoadId);
        }

        [Theory, AutoMoqData]
        public void MoveMessage(int distance)
        {
            AssertSerializable<Move>();

            var sut = new Move
            {
                Distance = distance
            };
            Assert.Equal(distance, sut.Distance);

            sut = new Move(distance);
            Assert.Equal(distance, sut.Distance);
        }

        // TODO: Write tests for MovementUpdate and PlayerPosition messages.
    }
}
