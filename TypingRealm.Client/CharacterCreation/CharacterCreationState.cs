using TypingRealm.Client.Typing;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed class CharacterCreationState
    {
        public CharacterCreationState(
            ITyperPool typerPool,
            ComponentPool componentPool)
        {
            CharacterNameInput = componentPool.MakeInputComponent();
            (BackToMainMenu, _) = typerPool.MakeUniqueTyper();
            (CreateCharacter, _) = typerPool.MakeUniqueTyper();
        }

        public Typer BackToMainMenu { get; }
        public InputComponent CharacterNameInput { get; }
        public Typer CreateCharacter { get; }

        public bool CreateCharacterButtonEnabled => CharacterNameInput.Value.Length >= 3;
    }
}
