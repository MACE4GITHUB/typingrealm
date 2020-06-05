using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain
{
    public interface IPlayerFactory
    {
        Player CreateNew(PlayerId playerId, PlayerName name);
        Player Create(PlayerId playerId, PlayerName name, LocationId locationId, RoadId? roadId, int? distance, PlayerId? combatEnemyId);
    }

    public sealed class PlayerFactory : IPlayerFactory
    {
        private readonly ILocationStore _locationStore;
        private readonly IRoadStore _roadStore;

        public PlayerFactory(
            ILocationStore locationStore,
            IRoadStore roadStore)
        {
            _locationStore = locationStore;
            _roadStore = roadStore;
        }

        public Player Create(PlayerId playerId, PlayerName name, LocationId locationId, RoadId? roadId, int? distance, PlayerId? combatEnemyId)
        {
            return new Player(playerId, name, locationId, _locationStore, roadId, distance, _roadStore, combatEnemyId);
        }

        public Player CreateNew(PlayerId playerId, PlayerName name)
        {
            var startingLocationId = _locationStore.GetStartingLocationId();

            return new Player(playerId, name, startingLocationId, _locationStore, null, null, _roadStore, null);
        }
    }
}
