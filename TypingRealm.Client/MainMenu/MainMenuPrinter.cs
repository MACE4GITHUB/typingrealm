using System.Collections.Generic;
using System.Linq;
using TypingRealm.Client.Data;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuPrinter : IPrinter<MainMenuPrinter.State>
    {
        public sealed record State(
            ITyperInformation CreateCharacterTyper,
            Dictionary<string, ITyperInformation> ConnectAsCharacterTypers,
            string WorldStateJson);

        private readonly IOutput _output;
        private readonly ICharacterService _characterService;

        public MainMenuPrinter(IOutput output, ICharacterService characterService)
        {
            _output = output;
            _characterService = characterService;
        }

        public void Print(State state)
        {
            _output.WriteLine("Main menu:");
            _output.WriteLine();
            _output.Write("Create new character");
            _output.Write(new string(' ', 10));
            _output.Write(state.CreateCharacterTyper);
            _output.WriteLine();

            if (!state.ConnectAsCharacterTypers.Any())
                return;

            _output.WriteLine("You have these characters:");
            _output.WriteLine();

            foreach (var character in state.ConnectAsCharacterTypers)
            {
                var characterId = character.Key;
                var typerInfo = character.Value;

                var characterName = _characterService.GetCharacterName(characterId);

                _output.Write(characterName);
                _output.Write(new string(' ', 10));
                _output.Write(typerInfo);
            }

            _output.WriteLine("World state:");
            _output.WriteLine(state.WorldStateJson);
        }
    }
}
