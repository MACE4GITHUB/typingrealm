namespace TypingRealm.Client.Typing;

public interface ITyperPool
{
    Typer? GetByKey(string key);
    Typer? GetTyper(char firstLetter);
    string? GetKeyFor(Typer typer);
    Typer MakeUniqueTyper(string key);
    void ResetUniqueTyper(Typer typer);
    void RemoveTyper(Typer typer);
}
