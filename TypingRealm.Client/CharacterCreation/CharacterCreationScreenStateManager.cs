using System;
using System.Reactive.Subjects;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed class CharacterCreationScreenStateManager : SyncManagedDisposable, IChangeDetector
    {
        private readonly object _updateStateLock = new object();
        private readonly ITyperPool _typerPool;

        private readonly CharacterCreationScreenState _currentState;
        private readonly BehaviorSubject<CharacterCreationScreenState> _stateSubject;

        public CharacterCreationScreenStateManager(ITyperPool typerPool)
        {
            _typerPool = typerPool;

            _currentState = CreateInitialState();
            InitializeTyperPool();
            _stateSubject = new BehaviorSubject<CharacterCreationScreenState>(_currentState);
        }

        public IObservable<CharacterCreationScreenState> StateObservable => _stateSubject;
        public void NotifyChanged() => _stateSubject.OnNext(_currentState);

        protected override void DisposeManagedResources()
        {
            _stateSubject.Dispose();
        }

        private CharacterCreationScreenState CreateInitialState()
        {
            return new CharacterCreationScreenState();
        }

        private void InitializeTyperPool()
        {
            _typerPool.MakeUniqueTyper("back");
        }
    }
}
