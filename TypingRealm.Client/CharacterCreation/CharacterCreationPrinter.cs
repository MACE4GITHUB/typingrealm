using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed class CharacterCreationPrinter : IPrinter<CharacterCreationScreenState>
    {
        private readonly IOutput _output;
        private readonly ITyperPool _typerPool;

        public CharacterCreationPrinter(
            IOutput output,
            ITyperPool typerPool)
        {
            _output = output;
            _typerPool = typerPool;
        }

        public void Print(CharacterCreationScreenState state)
        {
            _output.WriteLine("Character creation screen");
            _output.WriteLine();
            _output.Write("Go back to main menu");
            _output.Write(new string(' ', 10));
            // TODO: Don't use ! operator.
            _output.WriteLine(_typerPool.GetByKey("back")!);
            _output.WriteLine();
            _output.Write("Generate random character");
            // TODO: Don't use ! operator.
            _output.WriteLine(_typerPool.GetByKey("generate-random-character")!);
        }
    }
}
