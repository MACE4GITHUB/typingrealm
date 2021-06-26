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
    public sealed class MainMenuScreenStateManager : SyncManagedDisposable, IChangeDetector
    {
        private readonly object _updateStateLock = new object();
        private readonly ICharactersClient _charactersClient;

        private readonly MainMenuScreenState _currentState;
        private readonly BehaviorSubject<MainMenuScreenState> _stateSubject;

        public MainMenuScreenStateManager(
            ICharactersClient charactersClient,
            MainMenuModel model)
        {
            _charactersClient = charactersClient;

            _currentState = CreateInitialState(model);
            _stateSubject = new BehaviorSubject<MainMenuScreenState>(_currentState);

            _ = GetCharacters(); // Fire and forget.

            // HACK to make characters update on the main screen.
            Observable.Interval(TimeSpan.FromSeconds(5)).Subscribe(_ => GetCharacters());
        }

        public IObservable<MainMenuScreenState> StateObservable => _stateSubject;

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

        private static MainMenuScreenState CreateInitialState(MainMenuModel model)
        {
            return new MainMenuScreenState(model);
        }

        private void UpdateCharacters(IEnumerable<CharacterResource> characters)
        {
            lock (_updateStateLock)
            {
                foreach (var character in _currentState.Characters.Where(c => !characters.Any(x => x.CharacterId == c.CharacterId)))
                {
                    _currentState.Model.RemoveSelectCharacterTyper(character.CharacterId);
                }

                foreach (var character in characters)
                {
                    _currentState.Model.SyncSelectCharacterTyper(character.CharacterId);
                }

                _currentState.Characters = characters
                    .Select(x => CharacterInfo.FromCharacterResource(x))
                    .ToList();

                _stateSubject.OnNext(_currentState);
            }
        }
    }
}
