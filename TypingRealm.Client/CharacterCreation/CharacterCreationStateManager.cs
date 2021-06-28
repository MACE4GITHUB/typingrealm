using System;
using System.Reactive.Subjects;
using TypingRealm.Client.Interaction;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed class CharacterCreationStateManager : SyncManagedDisposable, IChangeDetector
    {
        private readonly object _updateStateLock = new object();

        private readonly CharacterCreationState _currentState;
        private readonly BehaviorSubject<CharacterCreationState> _stateSubject;

        public CharacterCreationStateManager(
            CharacterCreationState state)
        {
            _currentState = state;
            _stateSubject = new BehaviorSubject<CharacterCreationState>(_currentState);
        }

        public IObservable<CharacterCreationState> StateObservable => _stateSubject;
        public void NotifyChanged() => _stateSubject.OnNext(_currentState);

        protected override void DisposeManagedResources()
        {
            _stateSubject.Dispose();
        }
    }
}
