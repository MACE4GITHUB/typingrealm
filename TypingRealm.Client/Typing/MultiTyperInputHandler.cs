using TypingRealm.Client.Interaction;

namespace TypingRealm.Client.Typing
{
    public abstract class MultiTyperInputHandler : IInputHandler
    {
        private Typer? _focusedTyper;

        protected MultiTyperInputHandler(ITyperPool typerPool)
        {
            TyperPool = typerPool;
        }

        protected ITyperPool TyperPool { get; }

        public void Type(char character)
        {
            if (_focusedTyper == null)
            {
                var typer = TyperPool.GetTyper(character);
                if (typer == null)
                    return;

                _focusedTyper = typer;
            }

            _focusedTyper.Type(character);

            if (_focusedTyper.IsFinishedTyping)
            {
                TyperPool.ResetUniqueTyper(_focusedTyper);

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

        // TODO: Remove this and use Typer protected property.
        protected Typer MakeUniqueTyper() => TyperPool.MakeUniqueTyper().Typer;
    }
}
