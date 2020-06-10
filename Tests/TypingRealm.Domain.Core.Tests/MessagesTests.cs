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
            Assert.Equal(11, typeof(Join).Assembly.GetTypes().Count(
                t => t.GetCustomAttribute<MessageAttribute>() != null));
        }

        [Theory, AutoMoqData]
        public void JoinMessage(string name)
        {
            AssertSerializable<Join>();

            var sut = new Join(name);
            Assert.Equal(name, sut.PlayerId);
        }

        [Theory, AutoMoqData]
        public void MoveToMessage(string locationId)
        {
            AssertSerializable<MoveToLocation>();

            var sut = new MoveToLocation(locationId);
            Assert.Equal(locationId, sut.LocationId);
        }

        [Theory, AutoMoqData]
        public void UpdateMessage(
            string locationId,
            IEnumerable<string> visiblePlayers)
        {
            AssertSerializable<Update>();

            var sut = new Update(locationId, visiblePlayers);
            Assert.Equal(locationId, sut.LocationId);
            Assert.Equal(visiblePlayers, sut.VisiblePlayers);
        }

        [Theory, AutoMoqData]
        public void CombatUpdateMessage(string enemyId)
        {
            AssertSerializable<CombatUpdate>();

            var sut = new CombatUpdate(enemyId);
            Assert.Equal(enemyId, sut.EnemyId);
        }

        [Theory, AutoMoqData]
        public void AttackMessage(string playerId)
        {
            AssertSerializable<Attack>();

            var sut = new Attack(playerId);
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

            var sut = new EnterRoad(roadId);
            Assert.Equal(roadId, sut.RoadId);
        }

        [Theory, AutoMoqData]
        public void MoveMessage(int distance)
        {
            AssertSerializable<Move>();

            var sut = new Move(distance);
            Assert.Equal(distance, sut.Distance);
        }

        [Theory, AutoMoqData]
        public void MovementUpdateMessage(
            string roadId,
            PlayerPosition playerPosition,
            IEnumerable<PlayerPosition> playerPositions)
        {
            AssertSerializable<MovementUpdate>();

            var sut = new MovementUpdate(roadId, playerPosition, playerPositions);
            Assert.Equal(roadId, sut.RoadId);
            Assert.Equal(playerPosition, sut.PlayerPosition);
            Assert.Equal(playerPositions, sut.PlayerPositions);
        }

        [Theory, AutoMoqData]
        public void PlayerPositionMessage(
            string playerId,
            int progress,
            bool isMovingForward)
        {
            AssertSerializable<PlayerPosition>();

            var sut = new PlayerPosition(playerId, progress, isMovingForward);
            Assert.Equal(playerId, sut.PlayerId);
            Assert.Equal(progress, sut.Progress);
            Assert.Equal(isMovingForward, sut.IsMovingForward);
        }

        [Theory, AutoMoqData]
        public void TeleportPlayerToLocationMessage(
            string playerId,
            string locationId)
        {
            AssertSerializable<TeleportPlayerToLocation>();

            var sut = new TeleportPlayerToLocation(playerId, locationId);
            Assert.Equal(playerId, sut.PlayerId);
            Assert.Equal(locationId, sut.LocationId);
        }
    }
}
