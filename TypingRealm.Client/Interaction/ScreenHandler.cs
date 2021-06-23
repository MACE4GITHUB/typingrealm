using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.Interaction
{
    public abstract class ScreenHandler<TState> : MultiTyperInputHandler, IScreenHandler
    {
        private readonly IPrinter<TState> _printer;

        protected ScreenHandler(
            ITyperPool typerPool,
            IPrinter<TState> printer) : base(typerPool)
        {
            _printer = printer;
        }

        public void PrintState()
        {
            var state = GetCurrentScreenState();
            _printer.Print(state);
        }

        protected abstract TState GetCurrentScreenState();
    }
}
