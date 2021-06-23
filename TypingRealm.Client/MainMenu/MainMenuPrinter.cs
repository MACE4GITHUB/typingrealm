using System.Linq;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuPrinter : IPrinter<MainMenuScreenState>
    {
        private readonly IOutput _output;
        private readonly ITyperPool _typerPool;

        public MainMenuPrinter(
            IOutput output,
            ITyperPool typerPool)
        {
            _output = output;
            _typerPool = typerPool;
        }

        public void Print(MainMenuScreenState state)
        {
            _output.WriteLine("Main menu:");
            _output.WriteLine();
            _output.Write("Create new character");
            _output.Write(new string(' ', 10));
            // TODO: Don't use ! operator.
            _output.Write(_typerPool.GetByKey("create-character")!);
            _output.WriteLine();

            if (!state.Characters.Any())
                return;

            _output.WriteLine("You have these characters:");
            _output.WriteLine();

            foreach (var character in state.Characters)
            {
                // TODO: Don't use ! operator.
                var typer = _typerPool.GetByKey(character.CharacterId)!;

                _output.Write(character.Name);
                _output.Write(new string(' ', 10));
                _output.Write(typer);
            }
        }
    }
}
