using System;
using System.Linq;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain
{
    public enum PlayerState
    {
        AtLocation = 1,
        MovingOnRoad = 2
    }

    public sealed class Player
    {
        private readonly Action<string> _updateMessagingGroup;
        private readonly ILocationStore _locationStore;
        private readonly IRoadStore _roadStore;

        public Player(
            PlayerId playerId,
            LocationId locationId,
            ILocationStore locationStore,
            IRoadStore roadStore,
            Action<string> updateMessagingGroup)
        {
            PlayerId = playerId;
            _locationId = locationId;
            _locationStore = locationStore;
            _roadStore = roadStore;
            _updateMessagingGroup = updateMessagingGroup;

            State = PlayerState.AtLocation;
        }

        public PlayerState State { get; private set; }

        public PlayerId PlayerId { get; }

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
        private RoadMovementComponent? _movementComponent;
        public RoadMovementComponent? MovementComponent
        {
            get => _movementComponent;
            private set
            {
                _movementComponent = value;

                // RoadId if not null, LocationId if null.
                _updateMessagingGroup(PlayerUniquePosition);
            }
        }

        public void TeleportToLocation(LocationId locationId)
        {
            // Test method for TeleportPlayerToLocation message.
            // TODO: Remove, refactor or protect this method validating invariants.
            LocationId = locationId;
        }

        public void MoveToLocation(LocationId locationId)
        {
            if (State != PlayerState.AtLocation)
                throw new InvalidOperationException("Player is not in AtLocation state.");

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
            if (State != PlayerState.AtLocation)
                throw new InvalidOperationException("Player can enter road only from location.");

            if (MovementComponent != null)
                throw new InvalidOperationException("Already at road.");

            var road = _roadStore.Find(roadId);
            if (road == null)
                throw new InvalidOperationException("Road does not exist.");

            // Check that current location allows movement to this road.
            var location = _locationStore.Find(LocationId);
            if (location == null)
                throw new InvalidOperationException("Current location does not exist.");

            if (!location.Roads.Contains(roadId))
                throw new InvalidOperationException("Cannot move to this road from the current location.");

            MovementComponent = RoadMovementComponent.EnterRoadFrom(road, LocationId);

            State = PlayerState.MovingOnRoad;
        }

        public void Move(Distance distance)
        {
            if (State != PlayerState.MovingOnRoad)
                throw new InvalidOperationException("Not at road.");

            if (MovementComponent == null)
                throw new InvalidOperationException("Not at road.");

            MovementComponent = MovementComponent.Move(distance);

            if (MovementComponent.HasArrived)
            {
                LocationId = MovementComponent.ArrivalLocationId;
                MovementComponent = null;

                State = PlayerState.AtLocation;
            }
        }

        public void TurnAround()
        {
            if (State != PlayerState.MovingOnRoad)
                throw new InvalidOperationException("Not at road.");

            if (MovementComponent == null)
                throw new InvalidOperationException("Not at road.");

            MovementComponent = MovementComponent.TurnAround();

            if (MovementComponent.HasArrived)
            {
                LocationId = MovementComponent.ArrivalLocationId;
                MovementComponent = null;

                State = PlayerState.AtLocation;
            }
        }

        private string PlayerUniquePosition => MovementComponent?.Road.RoadId.Value ?? LocationId.Value;
    }
}
