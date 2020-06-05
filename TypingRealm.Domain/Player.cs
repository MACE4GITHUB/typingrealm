﻿using System;
using System.Linq;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain
{
    public sealed class Player
    {
        private readonly ILocationStore _locationStore;
        private readonly IRoadStore _roadStore;

        public Player(
            PlayerId playerId,
            PlayerName name,
            LocationId locationId,
            ILocationStore locationStore,
            RoadId? roadId,
            int? distance,
            IRoadStore roadStore,
            PlayerId? combatEnemyId)
        {
            PlayerId = playerId;
            Name = name;
            LocationId = locationId;
            _locationStore = locationStore;
            RoadId = roadId;
            Distance = distance;
            _roadStore = roadStore;
            CombatEnemyId = combatEnemyId;
        }

        public PlayerId PlayerId { get; }
        public PlayerName Name { get; }
        public LocationId LocationId { get; private set; }

        // Combat.
        public PlayerId? CombatEnemyId { get; private set; }

        // Movement.
        public RoadId? RoadId { get; private set; }
        public int? Distance { get; private set; }

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

        public void EnterRoad(RoadId roadId)
        {
            if (RoadId != null)
                throw new InvalidOperationException("Already at road.");

            var road = _roadStore.Find(roadId);
            if (road == null)
                throw new InvalidOperationException("Road does not exist.");

            if (road.FromLocationId != LocationId)
                throw new InvalidOperationException("Cannot move to this road from this location.");

            RoadId = roadId;
            Distance = 0;
        }

        public void Move(int distance)
        {
            if (RoadId == null)
                throw new InvalidOperationException("Not at road.");

            var road = _roadStore.Find(RoadId);
            if (road == null)
                throw new InvalidOperationException("Road does not exist.");

            if (Distance + distance > road.Distance)
                throw new InvalidOperationException("Can't move so much.");

            Distance += distance;

            if (Distance == road.Distance)
            {
                // Arrive.
                Distance = null;
                RoadId = null;
                LocationId = road.ToLocationId;
            }
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

        public void Surrender(IPlayerRepository playerRepository)
        {
            if (CombatEnemyId == null)
                throw new InvalidOperationException("Can't surrender: not in battle.");

            var enemy = playerRepository.Find(CombatEnemyId);
            if (enemy == null)
                throw new InvalidOperationException("Enemy is not found.");

            CombatEnemyId = null;
            enemy.CombatEnemyId = null;

            playerRepository.Save(this);
            playerRepository.Save(enemy);
        }

        public string GetUniquePlayerPosition()
        {
            return $"l_{LocationId}";
        }
    }
}
