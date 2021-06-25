﻿using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuInputHandler : MultiTyperInputHandler
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly IConnectionManager _connectionManager;
        private readonly MainMenuTypers _mainMenuTypers;

        public MainMenuInputHandler(
            ITyperPool typerPool,
            MainMenuTypers mainMenuTypers,
            IScreenNavigation screenNavigation,
            IConnectionManager connectionManager) : base(typerPool)
        {
            _screenNavigation = screenNavigation;
            _connectionManager = connectionManager;
            _mainMenuTypers = mainMenuTypers;
        }

        protected override void OnTyped(Typer typer)
        {
            if (typer == _mainMenuTypers.CreateCharacter)
            {
                SwitchToCharacterCreationScreen();
                return;
            }

            if (typer == _mainMenuTypers.Exit)
            {
                Exit();
                return;
            }

            var characterId = TyperPool.GetKeyFor(typer);
            if (characterId == null)
                return;

            // TODO: Refactor this (encapsulate).
            ConnectAsCharacter(characterId.Replace("select-character-", string.Empty));
        }

        private void SwitchToCharacterCreationScreen()
        {
            _screenNavigation.Screen = GameScreen.CharacterCreation;
        }

        private void ConnectAsCharacter(string characterId)
        {
            _connectionManager.ConnectToWorldAsync(characterId, default)
                .GetAwaiter().GetResult(); // TODO: Do not block like this.

            // Maybe set screen to some intermediatery "loading" value. and after connection screen should be determined by received state.
            // TODO: See if after connecting to world the character is actually in some activity - and select this activity screen instead of world screen.
            _screenNavigation.Screen = GameScreen.World;
        }

        private void Exit()
        {
            _screenNavigation.Screen = GameScreen.Exit;
        }
    }
}
