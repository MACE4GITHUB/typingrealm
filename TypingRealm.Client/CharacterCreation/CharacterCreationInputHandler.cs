using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.Profiles.Api.Resources.Data;

namespace TypingRealm.Client.CharacterCreation
{
    public sealed class CharacterCreationInputHandler : MultiTyperInputHandler
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly ICharactersClient _charactersClient;
        private readonly CharacterCreationState _state;

        public CharacterCreationInputHandler(
            ITyperPool typerPool,
            ComponentPool componentPool,
            CharacterCreationState state,
            IScreenNavigation screenNavigation,
            ICharactersClient charactersClient) : base(typerPool, componentPool)
        {
            _screenNavigation = screenNavigation;
            _charactersClient = charactersClient;
            _state = state;
        }

        protected override void OnTyped(Typer typer)
        {
            base.OnTyped(typer);

            if (typer == _state.BackToMainMenu)
            {
                SwitchBackToMainMenu();
                return;
            }

            if (typer == _state.CreateCharacter)
            {
                CreateCharacter();
                return;
            }
        }

        private void SwitchBackToMainMenu()
        {
            _screenNavigation.Screen = GameScreen.MainMenu;
        }

        private void CreateCharacter()
        {
            _charactersClient.CreateAsync(new CreateCharacterDto
            {
                Name = _state.CharacterNameInput.Value
            }, default).GetAwaiter().GetResult();

            _screenNavigation.Screen = GameScreen.MainMenu;
        }
    }
}
