namespace TypingRealm.Client.Typing
{
    public interface ITyperPool
    {
        Typer? GetById(string id);
        Typer? GetTyper(char firstLetter);
        Typer MakeUniqueTyper(string id);
        void ResetUniqueTyper(Typer typer);
        void RemoveTyper(Typer typer);
    }
}
