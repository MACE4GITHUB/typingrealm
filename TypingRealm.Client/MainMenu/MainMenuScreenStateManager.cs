using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using TypingRealm.Client.Typing;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Client.MainMenu
{
    public interface IChangeDetector
    {
        void NotifyChanged();
    }

    public sealed record ScreenDependencies<TManager, TPrinter, THandler>(
        TManager Manager,
        TPrinter Printer,
        THandler Handler);

    public sealed class MainMenuScreenStateManager : SyncManagedDisposable, IChangeDetector
    {
        private readonly object _updateStateLock = new object();
        private readonly ICharactersClient _charactersClient;
        private readonly ITyperPool _typerPool;

        private readonly MainMenuScreenState _currentState;
        private readonly BehaviorSubject<MainMenuScreenState> _stateSubject;

        public MainMenuScreenStateManager(
            ICharactersClient charactersClient,
            ITyperPool typerPool)
        {
            _charactersClient = charactersClient;
            _typerPool = typerPool;

            _currentState = CreateInitialState();
            InitializeTyperPool();
            _stateSubject = new BehaviorSubject<MainMenuScreenState>(_currentState);

            _ = GetCharacters(); // Fire and forget.
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

        private static MainMenuScreenState CreateInitialState()
        {
            return new MainMenuScreenState();
        }

        private void UpdateCharacters(IEnumerable<CharacterResource> characters)
        {
            lock (_updateStateLock)
            {
                foreach (var character in _currentState.Characters.Where(c => !characters.Any(x => x.CharacterId == c.CharacterId)))
                {
                    _typerPool.RemoveTyper(character.CharacterId);
                }

                foreach (var character in characters)
                {
                    var typer = _typerPool.GetByKey(character.CharacterId);
                    if (typer == null)
                        _typerPool.MakeUniqueTyper(character.CharacterId);
                }

                _currentState.Characters = characters
                    .Select(x => MainMenu.CharacterInfo.FromCharacterResource(x))
                    .ToList();

                _stateSubject.OnNext(_currentState);
            }
        }

        private void InitializeTyperPool()
        {
            _typerPool.MakeUniqueTyper("exit");
            _typerPool.MakeUniqueTyper("create-character");
        }
    }
}
