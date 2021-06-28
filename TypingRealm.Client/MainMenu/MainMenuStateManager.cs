using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using TypingRealm.Client.Interaction;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuStateManager : SyncManagedDisposable, IChangeDetector
    {
        private readonly object _updateStateLock = new object();
        private readonly ICharactersClient _charactersClient;

        private readonly MainMenuState _currentState;
        private readonly BehaviorSubject<MainMenuState> _stateSubject;

        public MainMenuStateManager(
            ICharactersClient charactersClient,
            MainMenuState state)
        {
            _charactersClient = charactersClient;

            _currentState = state;
            _stateSubject = new BehaviorSubject<MainMenuState>(_currentState);

            _ = GetCharacters(); // Fire and forget.

            // HACK to make characters update on the main screen.
            Observable.Interval(TimeSpan.FromSeconds(5)).Subscribe(_ => GetCharacters());
        }

        public IObservable<MainMenuState> StateObservable => _stateSubject;
        public void NotifyChanged() => _stateSubject.OnNext(_currentState);

        // Can be made private.
        public async Task GetCharacters()
        {
            var characters = await _charactersClient.GetAllByProfileIdAsync(default)
                .ConfigureAwait(false);

            UpdateCharacters(characters);
        }

        protected override void DisposeManagedResources()
        {
            _stateSubject.Dispose();
        }

        private void UpdateCharacters(IEnumerable<CharacterResource> characters)
        {
            lock (_updateStateLock)
            {
                foreach (var character in _currentState.Characters.Where(c => !characters.Any(x => x.CharacterId == c.CharacterId)))
                {
                    _currentState.RemoveSelectCharacterTyper(character.CharacterId);
                }

                foreach (var character in characters)
                {
                    _currentState.SyncSelectCharacterTyper(character.CharacterId, character.Name);
                }

                _stateSubject.OnNext(_currentState);
            }
        }
    }
}
