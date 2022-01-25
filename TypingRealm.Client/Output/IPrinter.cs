namespace TypingRealm.Client.Output;

public interface IPrinter<in TPrintState>
{
    void Print(TPrintState state);
}
