using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;
using TypingRealm.Messaging.Client;
using TypingRealm.Profiles.Activities;

namespace TypingRealm.Client.World
{
    public sealed class WorldScreenStateManager : SyncManagedDisposable, IChangeDetector
    {
        private readonly object _updateStateLock = new object();

        private readonly ITyperPool _typerPool;
        private readonly WorldScreenState _currentState;
        private readonly BehaviorSubject<WorldScreenState> _stateSubject;

        private readonly HashSet<string> _subscriptions = new HashSet<string>();
        private IMessageProcessor? _worldConnection;
        private readonly string _characterId;

        // TODO: Rewrite this whole logic so that all these dependencies are created PER PAGE when it opens, and are disposed when the page closes.
        public WorldScreenStateManager(
            ITyperPool typerPool,
            IConnectionManager connectionManager,
            WorldScreenState state)
        {
            _characterId = connectionManager.CharacterId;
            _typerPool = typerPool;

            connectionManager.WorldConnectionObservable.Subscribe(worldConnection =>
            {
                // TODO: Cleanup - remove and dispose all subscriptions / previous connection.

                if (worldConnection == null)
                    return;

                _worldConnection = worldConnection;

                _subscriptions.Add(_worldConnection.Subscribe<TypingRealm.World.WorldState>(state =>
                {
                    // TODO: This is VERY bad, potential issue if this action is awaited.
                    // Sync lock doesn't work with async api.
                    lock (_updateStateLock)
                    {
                        UpdateState(state);
                        return default;
                    }
                }));
            });

            _worldConnection = connectionManager.WorldConnection;

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
            foreach (var subscription in _subscriptions)
            {
                _worldConnection?.Unsubscribe(subscription);
            }

            _stateSubject.Dispose();
        }

        private void UpdateState(TypingRealm.World.WorldState state)
        {
            lock (_updateStateLock)
            {
                _currentState.CurrentLocation = new LocationInfo(
                    state.LocationId,
                    "TODO: get location name from cache",
                    "TODO: get location description from cache");

                if (state.AllowedActivityTypes.Contains(ActivityType.RopeWar)
                    && _currentState.CreateRopeWarTyper == null)
                {
                    _currentState.CreateRopeWarTyper = _typerPool.MakeUniqueTyper(Guid.NewGuid().ToString());
                }

                // TODO: Dispose all previous location entrances - sync up like at character selection screen.
                _currentState.LocationEntrances = new HashSet<LocationEntrance>(state.Locations.Select(
                    x => new LocationEntrance(x, x, _typerPool.MakeUniqueTyper(Guid.NewGuid().ToString()))));

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
    }
}
