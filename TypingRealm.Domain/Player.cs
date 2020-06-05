﻿using System;
using System.Linq;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain
{
    public sealed class Player
    {
        private readonly ILocationStore _locationStore;

        public Player(PlayerId playerId, PlayerName name, LocationId locationId, ILocationStore locationStore, PlayerId? combatEnemyId)
        {
            PlayerId = playerId;
            Name = name;
            LocationId = locationId;
            _locationStore = locationStore;
            CombatEnemyId = combatEnemyId;
        }

        public PlayerId PlayerId { get; }
        public PlayerName Name { get; }
        public LocationId LocationId { get; private set; }
        public PlayerId? CombatEnemyId { get; private set; }

        public void MoveToLocation(LocationId locationId)
        {
            if (LocationId == locationId)
                throw new InvalidOperationException($"Player {PlayerId} is already at location {LocationId}. Cannot move to the same location.");

            var currentLocation = _locationStore.Find(LocationId);
            if (currentLocation == null)
                throw new InvalidOperationException($"The player {PlayerId} is currently at invalid location {LocationId}. Cannot move to location {locationId}.");

            if (_locationStore.Find(locationId) == null)
                throw new InvalidOperationException($"Location {locationId} doesn't exist.");

            if (!currentLocation.Locations.Contains(locationId))
                throw new InvalidOperationException($"Cannot move the player {PlayerId} from {LocationId} to {locationId}.");

            LocationId = locationId;
        }

        public void Attack(Player player)
        {
            if (CombatEnemyId != null)
                throw new InvalidOperationException("Attacker is already in battle.");

            if (player.CombatEnemyId != null)
                throw new InvalidOperationException("Enemy is already in battle.");

            CombatEnemyId = player.PlayerId;
            player.CombatEnemyId = PlayerId;
        }

        public string GetUniquePlayerPosition()
        {
            return $"l_{LocationId}";
        }
    }
}
