using System;
using System.Linq;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain
{
    public sealed class Player
    {
        private readonly Action<string> _updateMessagingGroup;
        private readonly ILocationStore _locationStore;
        private readonly IRoadStore _roadStore;

        public Player(
            PlayerId playerId,
            PlayerName name,
            LocationId locationId,
            ILocationStore locationStore,
            IRoadStore roadStore,
            PlayerId? combatEnemyId,
            Action<string> updateMessagingGroup)
        {
            PlayerId = playerId;
            Name = name;
            _locationId = locationId;
            _locationStore = locationStore;
            _roadStore = roadStore;
            CombatEnemyId = combatEnemyId;
            _updateMessagingGroup = updateMessagingGroup;
        }

        public PlayerId PlayerId { get; }
        public PlayerName Name { get; }

        private LocationId _locationId;
        public LocationId LocationId
        {
            get => _locationId;
            private set
            {
                _locationId = value;
                _updateMessagingGroup(PlayerUniquePosition);
            }
        }

        // Combat.
        public PlayerId? CombatEnemyId { get; private set; }

        // Movement.
        private MovementComponent? _movementComponent;
        public MovementComponent? MovementComponent
        {
            get => _movementComponent;
            private set
            {
                _movementComponent = value;

                // RoadId if not null, LocationId if null.
                _updateMessagingGroup(PlayerUniquePosition);
            }
        }

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
            if (MovementComponent != null)
                throw new InvalidOperationException("Already at road.");

            var road = _roadStore.Find(roadId);
            if (road == null)
                throw new InvalidOperationException("Road does not exist.");

            MovementComponent = MovementComponent.EnterRoadFrom(road, LocationId);
        }

        public void Move(Distance distance)
        {
            if (MovementComponent == null)
                throw new InvalidOperationException("Not at road.");

            MovementComponent = MovementComponent.Move(distance);

            if (MovementComponent.HasArrived)
            {
                LocationId = MovementComponent.ArrivalLocationId;
                MovementComponent = null;
            }
        }

        public void TurnAround()
        {
            if (MovementComponent == null)
                throw new InvalidOperationException("Not at road.");

            MovementComponent = MovementComponent.TurnAround();

            if (MovementComponent.HasArrived)
            {
                LocationId = MovementComponent.ArrivalLocationId;
                MovementComponent = null;
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

        private string PlayerUniquePosition => MovementComponent?.Road.RoadId.Value ?? LocationId.Value;
    }
}
