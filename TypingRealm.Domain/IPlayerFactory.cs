﻿using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain
{
    public interface IPlayerFactory
    {
        Player CreateNew(PlayerId playerId, PlayerName name);
        Player Create(PlayerId playerId, PlayerName name, LocationId locationId, PlayerId? combatEnemyId);
    }

    public sealed class PlayerFactory : IPlayerFactory
    {
        private readonly ILocationStore _locationStore;

        public PlayerFactory(ILocationStore locationStore)
        {
            _locationStore = locationStore;
        }

        public Player Create(PlayerId playerId, PlayerName name, LocationId locationId, PlayerId? combatEnemyId)
        {
            return new Player(playerId, name, locationId, _locationStore, combatEnemyId);
        }

        public Player CreateNew(PlayerId playerId, PlayerName name)
        {
            var startingLocationId = _locationStore.GetStartingLocationId();

            return new Player(playerId, name, startingLocationId, _locationStore, null);
        }
    }
}
