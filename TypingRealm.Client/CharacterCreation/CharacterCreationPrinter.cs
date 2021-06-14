using TypingRealm.Client.Output;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed class CharacterCreationPrinter : IPrinter<CharacterCreationPrintableState>
    {
        private readonly IOutput _output;

        public CharacterCreationPrinter(IOutput output)
        {
            _output = output;
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
