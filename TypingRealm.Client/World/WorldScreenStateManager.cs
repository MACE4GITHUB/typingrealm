using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;
using TypingRealm.Messaging.Client;
using TypingRealm.World;

namespace TypingRealm.Client.World
{
    public sealed class WorldScreenStateManager : SyncManagedDisposable, IChangeDetector
    {
        private readonly object _updateStateLock = new object();
        private readonly HashSet<string> _subscriptions = new HashSet<string>();
        private readonly ITyperPool _typerPool;

        private readonly BehaviorSubject<WorldScreenState?> _stateSubject;
        private WorldScreenState? _currentState;

        private IMessageProcessor? _worldConnection;

        // TODO: Rewrite this whole logic so that all these dependencies are created PER PAGE when it opens, and are disposed when the page closes.
        public WorldScreenStateManager(
            ITyperPool typerPool,
            IConnectionManager connectionManager)
        {
            connectionManager.WorldConnectionObservable.Subscribe(worldConnection =>
            {
                // TODO: Cleanup - remove and dispose all subscriptions / previous connection.

                if (worldConnection == null)
                    return;

                _worldConnection = worldConnection;

                _subscriptions.Add(_worldConnection.Subscribe<WorldState>(state =>
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
            _typerPool = typerPool;

            InitializeTyperPool();
            _stateSubject = new BehaviorSubject<WorldScreenState?>(null);
        }

        public IObservable<WorldScreenState?> StateObservable => _stateSubject;

        public void NotifyChanged() => _stateSubject.OnNext(_currentState);

        protected override void DisposeManagedResources()
        {
            foreach (var subscription in _subscriptions)
            {
                _worldConnection?.Unsubscribe(subscription);
            }

            _stateSubject.Dispose();
        }

        private void InitializeTyperPool()
        {
            _typerPool.MakeUniqueTyper("disconnect");
        }

        private void UpdateState(WorldState state)
        {
            lock (_updateStateLock)
            {
                if (_currentState == null)
                    InitializeState(state);
                else
                {
                    _currentState.CurrentLocation = new LocationInfo(
                        state.LocationId,
                        "TODO: get location name from cache",
                        "TODO: get location description from cache");
                }

                _stateSubject.OnNext(_currentState);
            }
        }

        private void InitializeState(WorldState state)
        {
            _currentState = new WorldScreenState(new LocationInfo(
                state.LocationId,
                "some initial name",
                "some initial description"));
        }
    }
}
