using TypingRealm.Client.Interaction;

namespace TypingRealm.Client.Typing
{
    public interface IInputComponent : IInputHandler
    {
        string Value { get; }
        bool IsFocused { get; set; } // TODO: Remove public setter.
        ITyperInformation FocusTyper { get; }
    }
}
