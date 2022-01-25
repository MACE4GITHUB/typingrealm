namespace TypingRealm.Client.Interaction;

public interface IInputHandler
{
    void Type(char character);
    void Backspace();
    void Escape();
    void Tab();
}
