using System.Collections.Generic;
using System.Linq;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuScreenHandler : ScreenHandler<MainMenuPrinter.State>
    {
        private readonly Typer _createCharacterTyper;
        private readonly Dictionary<string, Typer> _connectAsCharacterTypers
            = new Dictionary<string, Typer>();

        private readonly IMainMenuHandler _handler;

        public MainMenuScreenHandler(
            ITextGenerator textGenerator,
            IMainMenuHandler handler,
            IPrinter<MainMenuPrinter.State> printer) : base(textGenerator, printer)
        {
            _createCharacterTyper = MakeUniqueTyper();
            _handler = handler;
        }

        public void UpdateState(MainMenuState state)
        {
            var toRemove = new List<string>();
            foreach (var existing in _connectAsCharacterTypers)
            {
                if (state.Characters.Select(c => c.CharacterId).Contains(existing.Key))
                    continue;

                toRemove.Add(existing.Key);
            }

            foreach (var character in toRemove)
            {
                _connectAsCharacterTypers.Remove(character);
            }

            foreach (var character in state.Characters)
            {
                if (!_connectAsCharacterTypers.ContainsKey(character.CharacterId))
                    _connectAsCharacterTypers.Add(character.CharacterId, MakeUniqueTyper());
            }
        }

        protected override MainMenuPrinter.State GetCurrentScreenState()
        {
            return new MainMenuPrinter.State(_createCharacterTyper, _connectAsCharacterTypers.ToDictionary(x => x.Key, x => (ITyperInformation)x.Value));
        }

        protected override void OnTyped(Typer typer)
        {
            if (typer == _createCharacterTyper)
            {
                _handler.SwitchToCharacterCreationScreen();
                return;
            }

            var characterId = _connectAsCharacterTypers.First(x => x.Value == typer).Key;
            _handler.ConnectAsCharacter(characterId);
        }
    }
}
