using System;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Domain.Movement;
using TypingRealm.Domain.Tests.Customizations;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Domain.Tests
{
    public class PlayerTests : TestsBase
    {
        public PlayerTests()
        {
            Fixture.Customize(new DomainCustomization());
        }

        [Theory, InBattleAutoDomainData]
        public void ShouldSetFields(
            PlayerId playerId,
            PlayerName name,
            LocationId locationId,
            ILocationStore locationStore,
            PlayerId combatEnemyId)
        {
            var player = new Player(playerId, name, locationId, locationStore, null, null, null!, combatEnemyId);

            Assert.Equal(playerId, player.PlayerId);
            Assert.Equal(name, player.Name);
            Assert.Equal(locationId, player.LocationId);
            Assert.Equal(locationStore, GetPrivateField(player, "_locationStore"));
            Assert.Equal(combatEnemyId, player.CombatEnemyId);

            // Should allow null CombatEnemyId.
            player = new Player(playerId, name, locationId, locationStore, null, null, null!, null);
            Assert.Null(player.CombatEnemyId);
        }

        [Theory, RoamingAutoDomainData]
        public void ShouldGetUniquePlayerPosition(Player player)
        {
            Assert.Equal($"l_{player.LocationId}", player.GetUniquePlayerPosition());
        }

        [Theory, RoamingAutoDomainData]
        public void MoveToLocation_ShouldChangeLocation(
            [Frozen]Mock<ILocationStore> store,
            LocationId locationId,
            Player player)
        {
            var location = Create<Location>(
                new LocationWithAnotherLocation(locationId));

            store.Setup(x => x.Find(player.LocationId))
                .Returns(location);

            player.MoveToLocation(locationId);
            Assert.Equal(locationId, player.LocationId);
        }

        [Theory, RoamingAutoDomainData]
        public void MoveToLocation_ShouldThrow_WhenAlreadyAtLocation(
            [Frozen]Mock<ILocationStore> store,
            Player player)
        {
            var location = Create<Location>(
                new LocationWithAnotherLocation(player.LocationId));

            store.Setup(x => x.Find(player.LocationId))
                .Returns(location);

            Assert.Throws<InvalidOperationException>(
                () => player.MoveToLocation(player.LocationId));
        }

        [Theory, RoamingAutoDomainData]
        public void MoveToLocation_ShouldThrow_WhenCurrentLocationDoesNotExist(
            [Frozen]Mock<ILocationStore> store,
            Player player)
        {
            store.Setup(x => x.Find(player.LocationId))
                .Returns<Location>(null);

            Assert.Throws<InvalidOperationException>(
                () => player.MoveToLocation(Create<LocationId>()));
        }

        [Theory, RoamingAutoDomainData]
        public void MoveToLocation_ShouldThrow_WhenLocationDoesNotExist(
            [Frozen]Mock<ILocationStore> store,
            Player player,
            LocationId locationId)
        {
            var location = Create<Location>(
                new LocationWithAnotherLocation(locationId));

            store.Setup(x => x.Find(player.LocationId))
                .Returns(location);

            store.Setup(x => x.Find(locationId))
                .Returns<Location>(null);

            Assert.Throws<InvalidOperationException>(
                () => player.MoveToLocation(locationId));
        }

        [Theory, RoamingAutoDomainData]
        public void MoveToLocation_ShouldThrow_WhenCannotMoveToThisLocationFromCurrentLocation(
            Player player,
            LocationId locationId)
        {
            Assert.Throws<InvalidOperationException>(
                () => player.MoveToLocation(locationId));
        }

        [Theory, RoamingAutoDomainData]
        public void Attack_ShouldSetCombatEnemyIdToBothPlayers(
            Player enemy,
            Player sut)
        {
            sut.Attack(enemy);

            Assert.Equal(enemy.PlayerId, sut.CombatEnemyId);
            Assert.Equal(sut.PlayerId, enemy.CombatEnemyId);
        }

        [Theory, InBattleAutoDomainData]
        public void Attack_ShouldThrow_WhenAttackerAlreadyInBattle(Player sut)
        {
            var enemy = Create<Player>(new PlayerNotInBattle());

            Assert.Throws<InvalidOperationException>(() => sut.Attack(enemy));
        }

        [Theory, InBattleAutoDomainData]
        public void Attack_ShouldThrow_WhenEnemyAlreadyInBattle(Player enemy)
        {
            var sut = Create<Player>(new PlayerNotInBattle());

            Assert.Throws<InvalidOperationException>(() => sut.Attack(enemy));
        }

        [Theory, RoamingAutoDomainData]
        public void Surrender_ShouldThrow_WhenNotInBattle(Player sut)
        {
            Assert.Throws<InvalidOperationException>(
                () => sut.Surrender(Create<IPlayerRepository>()));
        }

        [Theory, InBattleAutoDomainData]
        public void Surrender_ShouldStopBattleForBothPlayers(
            IPlayerRepository playerRepository,
            Player sut)
        {
            var enemy = Create<Player>(new PlayerInBattleWith(sut.PlayerId));
            Assert.NotNull(sut.CombatEnemyId);
            Assert.NotNull(enemy.CombatEnemyId);

            Mock.Get(playerRepository).Setup(x => x.Find(sut.CombatEnemyId!))
                .Returns(enemy);

            sut.Surrender(playerRepository);

            Assert.Null(sut.CombatEnemyId);
            Assert.Null(enemy.CombatEnemyId);
        }

        [Theory, InBattleAutoDomainData]
        public void Surrender_ShouldThrow_WhenEnemyNotFound(
            IPlayerRepository playerRepository,
            Player sut)
        {
            Mock.Get(playerRepository).Setup(x => x.Find(sut.CombatEnemyId!))
                .Returns<Player>(null);

            Assert.Throws<InvalidOperationException>(
                () => sut.Surrender(playerRepository));
        }
    }
}
