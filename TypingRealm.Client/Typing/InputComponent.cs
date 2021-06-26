namespace TypingRealm.Client.Typing
{
    public sealed class InputComponent : IInputComponent
    {
        public InputComponent(Typer focusTyper)
        {
            FocusTyper = focusTyper;
        }

        public Typer FocusTyper { get; }

        public string Value { get; private set; } = string.Empty;

        public bool IsFocused { get; set; } // TODO: Remove public setter.

        ITyperInformation IInputComponent.FocusTyper => FocusTyper;

        public void Backspace()
        {
            if (Value.Length > 0)
                Value = Value[0..^1];
        }

        public void Escape()
        {
            IsFocused = false; // This will notify the level above that we need to unfocus this component.
        }

        public void Tab()
        {
            // TODO: Move to the next component on this screen.
            // TODO: Also implement Shift+TAB.
        }

        public void Type(char character)
        {
            Value += character;
        }
    }
}
