using TypingRealm.Client.Interaction;

namespace TypingRealm.Client.Typing
{
    public abstract class MultiTyperInputHandler : IInputHandler
    {
        private Typer? _focusedTyper;
        private readonly ComponentPool? _componentPool;

        protected MultiTyperInputHandler(
            ITyperPool typerPool,
            ComponentPool? componentPool = null)
        {
            TyperPool = typerPool;
            _componentPool = componentPool;
        }

        protected ITyperPool TyperPool { get; }

        public void Type(char character)
        {
            var focusedComponent = _componentPool?.FocusedComponent;
            if (focusedComponent != null)
            {
                // Only work with the component.
                focusedComponent.Type(character);

                // TODO: Don't use ! operator.
                if (focusedComponent.IsFocused == false)
                    _componentPool!.FocusedComponent = null;

                return;
            }



            if (_focusedTyper == null)
            {
                var typer = TyperPool.GetTyper(character);
                if (typer == null || IsTyperDisabled(typer))
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
            var focusedComponent = _componentPool?.FocusedComponent;
            if (focusedComponent != null)
            {
                // Only work with the component.
                focusedComponent.Backspace();

                // TODO: Don't use ! operator.
                if (focusedComponent.IsFocused == false)
                    _componentPool!.FocusedComponent = null;

                return;
            }



            if (_focusedTyper == null)
                return;

            _focusedTyper.Backspace();

            if (!_focusedTyper.IsStartedTyping)
                _focusedTyper = null;
        }

        public void Escape()
        {
            var focusedComponent = _componentPool?.FocusedComponent;
            if (focusedComponent != null)
            {
                // Only work with the component.
                focusedComponent.Escape();

                // TODO: Don't use ! operator.
                if (focusedComponent.IsFocused == false)
                    _componentPool!.FocusedComponent = null;

                return;
            }



            if (_focusedTyper != null)
            {
                _focusedTyper.Reset();
                _focusedTyper = null;
            }
        }

        public virtual void Tab()
        {
            var focusedComponent = _componentPool?.FocusedComponent;
            if (focusedComponent != null)
            {
                // Only work with the component.
                focusedComponent.Tab();

                // TODO: Don't use ! operator.
                if (focusedComponent.IsFocused == false)
                    _componentPool!.FocusedComponent = null;

                return;
            }

            // Do nothing.
        }

        protected virtual void OnTyped(Typer typer)
        {
            _componentPool?.TestTyped(typer);
        }

        protected virtual bool IsTyperDisabled(Typer typer)
        {
            return false;
        }

        // TODO: Remove this and use Typer protected property.
        protected Typer MakeUniqueTyper() => TyperPool.MakeUniqueTyper().Typer;
    }
}
