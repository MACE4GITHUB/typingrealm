namespace TypingRealm.Client.Interaction
{
    public interface IScreenProvider
    {
        IInputHandler GetCurrentInputHandler();
        IChangeDetector GetCurrentChangeDetector();
    }
}
