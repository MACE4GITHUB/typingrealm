using TypingRealm.Client.Interaction;

namespace TypingRealm.Client.Typing
{
    public abstract class MultiTyperInputHandler : IInputHandler
    {
        private readonly ITyperPool _typerPool;
        private Typer? _focusedTyper;

        protected MultiTyperInputHandler(ITyperPool typerPool)
        {
            _typerPool = typerPool;
        }

        public void Type(char character)
        {
            if (_focusedTyper == null)
            {
                var typer = _typerPool.GetTyper(character);
                if (typer == null)
                    return;

                _focusedTyper = typer;
            }

            _focusedTyper.Type(character);

            if (_focusedTyper.IsFinishedTyping)
            {
                _typerPool.ResetUniqueTyper(_focusedTyper);

                OnTyped(_focusedTyper);
                _focusedTyper = null;
            }
        }

        public void Backspace()
        {
            if (_focusedTyper == null)
                return;

            _focusedTyper.Backspace();

            if (!_focusedTyper.IsStartedTyping)
                _focusedTyper = null;
        }

        public void Escape()
        {
            if (_focusedTyper != null)
            {
                _focusedTyper.Reset();
                _focusedTyper = null;
            }
        }

        public virtual void Tab() { }

        protected abstract void OnTyped(Typer typer);

        protected Typer MakeUniqueTyper() => _typerPool.MakeUniqueTyper();
    }
}
