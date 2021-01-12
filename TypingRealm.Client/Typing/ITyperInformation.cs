namespace TypingRealm.Client.Typing
{
    public interface ITyperInformation
    {
        string Typed { get; }
        string Error { get; }
        string NotTyped { get; }
        bool IsStartedTyping { get; }
    }
}
