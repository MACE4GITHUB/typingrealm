using System;
using System.Linq;
using TypingRealm.Domain.Movement;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.Domain;
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
public sealed class PlayerState
{
    public PlayerId PlayerId { get; set; }
    public LocationId LocationId { get; set; }
    public RoadMovementState? RoadMovementState { get; set; }
}

public sealed class RoadMovementState
{
    public RoadId RoadId { get; set; }
    public MovementDirection MovementDirection { get; set; }
    public Distance Progress { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

public interface IPlayerPersistenceFactory
{
    Player FromState(PlayerState state);
}

public sealed class PlayerPersistenceFactory : IPlayerPersistenceFactory
{
    private readonly ILocationStore _locationStore;
    private readonly IRoadStore _roadStore;
    private readonly IConnectedClientStore _connectedClientStore;

    public PlayerPersistenceFactory(
        ILocationStore locationStore,
        IRoadStore roadStore,
        IConnectedClientStore connectedClientStore)
    {
        _locationStore = locationStore;
        _roadStore = roadStore;
        _connectedClientStore = connectedClientStore;
    }

    public Player FromState(PlayerState state)
    {
        var playerId = state.PlayerId;

        void updateMessagingGroup(string group)
        {
            var client = _connectedClientStore.Find(playerId);
            if (client == null)
                return;
            //throw new InvalidOperationException($"Client {playerId} is not connected currently. Cannot update their messaging group.");

            // Do not throw when client is not connected, we can get Players from repositories even if they are offline - to perform actions on them on behalf of other players.

            client.Group = group;
        }

        return Player.FromState(state, _locationStore, _roadStore, updateMessagingGroup);
    }
}

public sealed class Player
{
    private readonly ILocationStore _locationStore;
    private readonly IRoadStore _roadStore;
    private readonly Action<string> _updateMessagingGroup;

    private readonly PlayerId _playerId;
    private LocationId _locationId;
#pragma warning disable IDE0032 // Use auto property
    private RoadMovementComponent? _roadMovementComponent;
#pragma warning restore IDE0032 // Use auto property

    private Player(
        PlayerState state,
        ILocationStore locationStore,
        IRoadStore roadStore,
        Action<string> updateMessagingGroup)
    {
        _locationStore = locationStore;
        _roadStore = roadStore;
        _updateMessagingGroup = updateMessagingGroup;

        _playerId = state.PlayerId;
        _locationId = state.LocationId;

        if (state.RoadMovementState != null)
        {
            var road = roadStore.Find(state.RoadMovementState.RoadId);
            if (road == null)
                throw new InvalidOperationException($"Road {state.RoadMovementState.RoadId} is not found in the store.");

            _roadMovementComponent = new RoadMovementComponent(road, state.RoadMovementState.Progress, state.RoadMovementState.MovementDirection);
        }

        UpdateMessagingGroup();
    }

    public string MessagingGroup => _roadMovementComponent == null
        ? $"location_{_locationId}"
        : $"road_{_roadMovementComponent.Road.RoadId}";

    // HACK: For easier update generation. It contains helper methods.
    public RoadMovementComponent? RoadMovementComponent => _roadMovementComponent;

    public static Player FromState(
        PlayerState state,
        ILocationStore locationStore,
        IRoadStore roadStore,
        Action<string> updateMessagingGroup)
    {
        return new Player(
            state,
            locationStore,
            roadStore,
            updateMessagingGroup);
    }

    public PlayerState GetState()
    {
        return new PlayerState
        {
            PlayerId = _playerId,
            LocationId = _locationId,
            RoadMovementState = _roadMovementComponent == null ? null : new RoadMovementState
            {
                RoadId = _roadMovementComponent.Road.RoadId,
                MovementDirection = _roadMovementComponent.Direction,
                Progress = _roadMovementComponent.Progress
            }
        };
    }

    public void MoveToLocation(LocationId locationId)
    {
        if (_roadMovementComponent != null)
            throw new InvalidOperationException($"Player {_playerId} is at road {_roadMovementComponent.Road.RoadId}. Cannot enter location from here.");

        if (_locationId == locationId)
            throw new InvalidOperationException($"Player {_playerId} is already at location {_locationId}. Cannot move to the same location.");

        var currentLocation = _locationStore.Find(_locationId);
        if (currentLocation == null)
            throw new InvalidOperationException($"Player {_playerId} is currently at not existing location {_locationId}. Cannot move to location {locationId}.");

        if (_locationStore.Find(locationId) == null)
            throw new InvalidOperationException($"Location {locationId} doesn't exist.");

        if (!currentLocation.Locations.Contains(locationId))
            throw new InvalidOperationException($"Cannot move the player {_playerId} from {_locationId} to {locationId}. There is no path to this location from the current location.");

        _locationId = locationId;
        _updateMessagingGroup($"location_{locationId}");
    }

    public void EnterRoad(RoadId roadId)
    {
        if (_roadMovementComponent != null)
            throw new InvalidOperationException($"Player {_playerId} is already at road {_roadMovementComponent.Road.RoadId}.");

        var road = _roadStore.Find(roadId);
        if (road == null)
            throw new InvalidOperationException($"Road {roadId} does not exist.");

        var location = _locationStore.Find(_locationId);
        if (location == null)
            throw new InvalidOperationException($"Current location {_locationId} does not exist.");

        // Check that current location allows movement to this road.
        if (!location.Roads.Contains(roadId))
            throw new InvalidOperationException($"Cannot move to road {roadId} from the current location {_locationId}.");

        _roadMovementComponent = RoadMovementComponent.EnterRoadFrom(road, location.LocationId);
    }

    public void Move(Distance distance)
    {
        if (_roadMovementComponent == null)
            throw new InvalidOperationException($"Player {_playerId} is not moving on road currently.");

        _roadMovementComponent = _roadMovementComponent.Move(distance);

        if (_roadMovementComponent.HasArrived)
        {
            _locationId = _roadMovementComponent.ArrivalLocationId;
            _roadMovementComponent = null;

            UpdateMessagingGroup();
        }
    }

    public void TurnAround()
    {
        if (_roadMovementComponent == null)
            throw new InvalidOperationException($"Player {_playerId} is not moving on road currently.");

        _roadMovementComponent.TurnAround();

        if (_roadMovementComponent.HasArrived)
        {
            _locationId = _roadMovementComponent.ArrivalLocationId;
            _roadMovementComponent = null;

            UpdateMessagingGroup();
        }
    }

    public void TeleportToLocation(LocationId locationId)
    {
        // TODO: Remove this method or protect it from calls from anybody (authorize).
        // Currently it's used for testing purposes.

        _locationId = locationId;
        UpdateMessagingGroup();
    }

    private void UpdateMessagingGroup() => _updateMessagingGroup(MessagingGroup);
}
