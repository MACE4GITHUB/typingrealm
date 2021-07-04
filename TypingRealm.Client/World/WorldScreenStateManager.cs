using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using TypingRealm.Client.Interaction;
using TypingRealm.Messaging.Client;

namespace TypingRealm.Client.World
{
    public sealed class WorldScreenStateManager : SyncManagedDisposable, IChangeDetector
    {
        private readonly object _updateStateLock = new object();

        private readonly WorldScreenState _currentState;
        private readonly BehaviorSubject<WorldScreenState> _stateSubject;

        private readonly HashSet<string> _subscriptions = new HashSet<string>();
        private IMessageProcessor? _worldConnection;

        // TODO: Rewrite this whole logic so that all these dependencies are created PER PAGE when it opens, and are disposed when the page closes.
        public WorldScreenStateManager(
            IConnectionManager connectionManager,
            WorldScreenState state)
        {
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

                _stateSubject.OnNext(_currentState);
            }
        }
    }
}
