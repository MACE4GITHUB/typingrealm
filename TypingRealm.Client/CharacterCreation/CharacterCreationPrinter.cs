using TypingRealm.Client.Output;

namespace TypingRealm.Client.CharacterCreation;

public sealed class CharacterCreationPrinter : IPrinter<CharacterCreationState>
{
    private readonly IOutput _output;

    public CharacterCreationPrinter(IOutput output)
    {
        _output = output;
    }

    public void Print(CharacterCreationState state)
    {
        _output.WriteLine("Character creation screen");
        _output.WriteLine();
        _output.Write("Go back to main menu");
        _output.Write(new string(' ', 10));
        _output.WriteLine(state.BackToMainMenu);
        _output.WriteLine();
        _output.Write("Character name: ");
        _output.WriteLine(state.CharacterNameInput);

        if (state.CreateCharacterButtonEnabled)
        {
            _output.WriteLine();
            _output.Write("Create character: ");
            _output.Write(state.CreateCharacter);
        }
    }
}
