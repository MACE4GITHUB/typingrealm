using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.Interaction
{
    public abstract class ScreenHandler<TPrintState> : MultiTyperInputHandler, IScreenHandler
    {
        private readonly IPrinter<TPrintState> _printer;

        protected ScreenHandler(
            ITextGenerator textGenerator,
            IPrinter<TPrintState> printer) : base(textGenerator)
        {
            _printer = printer;
        }

        public void PrintState()
        {
            var state = CreatePrintState();
            _printer.Print(state);
        }

        protected abstract TPrintState CreatePrintState();
    }
}
