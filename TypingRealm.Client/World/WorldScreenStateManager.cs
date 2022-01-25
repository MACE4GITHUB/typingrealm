using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;
using TypingRealm.Data.Api.Client;
using TypingRealm.Profiles.Activities;

namespace TypingRealm.Client.World;

public sealed class WorldScreenStateManager : SyncManagedDisposable, IChangeDetector
{
    private readonly object _updateStateLock = new object();

    private readonly ITyperPool _typerPool;
    private readonly WorldScreenState _currentState;
    private readonly BehaviorSubject<WorldScreenState> _stateSubject;

    private readonly ILocationsClient _locationsClient;

    private readonly HashSet<string> _subscriptions = new HashSet<string>();
    private readonly string _characterId;

    // TODO: Rewrite this whole logic so that all these dependencies are created PER PAGE when it opens, and are disposed when the page closes.
    public WorldScreenStateManager(
        ITyperPool typerPool,
        IConnectionManager connectionManager,
        WorldScreenState state,
        ILocationsClient locationsClient)
    {
        _characterId = connectionManager.CharacterId;
        _typerPool = typerPool;
        _locationsClient = locationsClient;

        connectionManager.WorldStateObservable.Subscribe(state =>
        {
            if (state == null)
                return; // TODO: Show loading screen?

                lock (_updateStateLock)
            {
                UpdateState(state);
            }
        });

        _currentState = state;
        _stateSubject = new BehaviorSubject<WorldScreenState>(_currentState);
    }

    public IObservable<WorldScreenState> StateObservable => _stateSubject;

    public void NotifyChanged()
    {
        try
        {
            ThrowIfDisposed();

            _stateSubject.OnNext(_currentState);
        }
        catch (ObjectDisposedException)
        {
            // TODO: Log.
        }
    }

    protected override void DisposeManagedResources()
    {
        _stateSubject.Dispose();
    }

    private void UpdateState(TypingRealm.World.WorldState state)
    {
        lock (_updateStateLock)
        {
            if (state.LocationId != _currentState.CurrentLocation?.LocationId)
            {
                // Very crude implementation, goes to API all the time and blocks the thread.
                var locationData = _locationsClient.GetLocationAsync(state.LocationId, default)
                    .GetAwaiter().GetResult();

                // Update current location details only when moving to another location.
                _currentState.CurrentLocation = new LocationInfo(
                    state.LocationId,
                    locationData.Name,
                    locationData.Description);
                /*"TODO: get location name from cache",
                "TODO: get location description from cache");*/
            }

            if (state.AllowedActivityTypes.Contains(ActivityType.RopeWar)
                && _currentState.CreateRopeWarTyper == null)
            {
                _currentState.CreateRopeWarTyper = _typerPool.MakeUniqueTyper(Guid.NewGuid().ToString());
            }

            if (!state.AllowedActivityTypes.Contains(ActivityType.RopeWar)
                && _currentState.CreateRopeWarTyper != null)
            {
                _typerPool.RemoveTyper(_currentState.CreateRopeWarTyper);
                _currentState.CreateRopeWarTyper = null;
            }

            // TODO: Dispose all previous location entrances - sync up like at character selection screen.
            _currentState.LocationEntrances = new HashSet<LocationEntrance>(state.Locations.Select(
                x => new LocationEntrance(x, GetLocationName(x), _typerPool.MakeUniqueTyper(Guid.NewGuid().ToString()))));

            // TODO: Dispose all previous things - sync up like at character selection screen.
            _currentState.RopeWars = new HashSet<RopeWarInfo>(state.RopeWarActivities.Select(
                x => new RopeWarInfo(x.ActivityId, x.ActivityId, x.Bet, _typerPool.MakeUniqueTyper(Guid.NewGuid().ToString()))));

            var characterId = _characterId;
            var currentRopeWar = state.RopeWarActivities.FirstOrDefault(rw => rw.LeftSideParticipants.Contains(characterId) || rw.RightSideParticipants.Contains(characterId));

            if (currentRopeWar != null)
            {
                _currentState.CurrentRopeWar = new JoinedRopeWar(
                    _currentState.RopeWars.First(x => x.RopeWarId == currentRopeWar.ActivityId), _typerPool.MakeUniqueTyper(Guid.NewGuid().ToString()), _typerPool.MakeUniqueTyper(Guid.NewGuid().ToString()));
            }
            // TODO: Clean up these typers. When current rope war becomes null - we need to delete them from the typer pool.
            else
            {
                _currentState.CurrentRopeWar = null;
            }

            _stateSubject.OnNext(_currentState);
        }
    }

    private string GetLocationName(string locationId)
    {
        var location = _locationsClient.GetLocationAsync(locationId, default)
            .GetAwaiter().GetResult();

        return location.Name;
    }
}
