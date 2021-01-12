namespace TypingRealm.Client.Output
{
    public interface IPrinter<TPrintState>
    {
        void Print(TPrintState state);
    }
}
