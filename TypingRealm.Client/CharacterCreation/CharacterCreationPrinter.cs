using TypingRealm.Client.Data;
using TypingRealm.Client.Output;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed class CharacterCreationPrinter : IPrinter<CharacterCreationPrintableState>
    {
        private readonly IOutput _output;
        private readonly ICharacterService _characterService;

        public CharacterCreationPrinter(IOutput output, ICharacterService characterService)
        {
            _output = output;
            _characterService = characterService;
        }

        public void Print(CharacterCreationPrintableState state)
        {
            _output.WriteLine("Character creation screen");
            _output.WriteLine();
            _output.Write("Go back to main menu");
            _output.Write(new string(' ', 10));
            _output.Write(state.ReturnBackToMainMenuTyper);
        }
    }
}
