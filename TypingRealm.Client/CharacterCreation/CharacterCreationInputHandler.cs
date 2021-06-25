using System;
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

        public CharacterCreationInputHandler(
            ITyperPool typerPool,
            IScreenNavigation screenNavigation,
            ICharactersClient charactersClient) : base(typerPool)
        {
            _screenNavigation = screenNavigation;
            _charactersClient = charactersClient;
        }

        protected override void OnTyped(Typer typer)
        {
            if (typer == TyperPool.GetByKey("back"))
            {
                SwitchBackToMainMenu();
                return;
            }

            if (typer == TyperPool.GetByKey("generate-random-character"))
            {
                GenerateRandomCharacter();
                return;
            }
        }

        private void SwitchBackToMainMenu()
        {
            _screenNavigation.Screen = GameScreen.MainMenu;
        }

        private void GenerateRandomCharacter()
        {
            _charactersClient.CreateAsync(new CreateCharacterDto
            {
                Name = $"{new string('a', new Random().Next(1, 3))}-{new string('b', new Random().Next(1, 5))}-{new string('c', new Random().Next(2, 10))}"
            }, default).GetAwaiter().GetResult();

            _screenNavigation.Screen = GameScreen.MainMenu;
        }
    }
}
