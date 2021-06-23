namespace TypingRealm.Client.Typing
{
    public interface ITyperPool
    {
        Typer? GetTyper(char firstLetter);
        Typer MakeUniqueTyper();
        void ResetUniqueTyper(Typer typer);
        void RemoveTyper(Typer typer);
    }
}
