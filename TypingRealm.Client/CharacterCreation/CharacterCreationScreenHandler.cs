using System;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed record CharacterCreationPrintableState(
        ITyperInformation ReturnBackToMainMenuTyper);

    public sealed class CharacterCreationScreenHandler : MultiTyperInputHandler, IScreenHandler
    {
        private readonly Typer _returnBackToMainMenuTyper;
        private readonly ICharacterCreationHandler _characterCreationHandler;
        private readonly IPrinter<CharacterCreationPrintableState> _printer;

        public CharacterCreationScreenHandler(
            ITextGenerator textGenerator,
            ICharacterCreationHandler characterCreationHandler,
            IPrinter<CharacterCreationPrintableState> printer)
            : base(textGenerator)
        {
            _returnBackToMainMenuTyper = MakeUniqueTyper();
            _characterCreationHandler = characterCreationHandler;
            _printer = printer;
        }

        public void PrintState()
        {
            var state = new CharacterCreationPrintableState(_returnBackToMainMenuTyper);
            _printer.Print(state);
        }

        protected override void OnTyped(Typer typer)
        {
            if (typer == _returnBackToMainMenuTyper)
            {
                _characterCreationHandler.CloseAndReturnToMainMenu();
                return;
            }

            throw new InvalidOperationException();
        }
    }
}
