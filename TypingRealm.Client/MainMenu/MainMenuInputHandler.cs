﻿using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuInputHandler : MultiTyperInputHandler
    {
        private readonly IScreenNavigation _screenNavigation;
        private readonly IConnectionManager _connectionManager;

        public MainMenuInputHandler(
            ITyperPool typerPool,
            IScreenNavigation screenNavigation,
            IConnectionManager connectionManager) : base(typerPool)
        {
            _screenNavigation = screenNavigation;
            _connectionManager = connectionManager;
        }

        protected override void OnTyped(Typer typer)
        {
            if (typer == TyperPool.GetByKey("create-character"))
            {
                SwitchToCharacterCreationScreen();
                return;
            }

            if (typer == TyperPool.GetByKey("exit"))
            {
                Exit();
                return;
            }

            // TODO: Do not use ! operator.
            var characterId = TyperPool.GetKeyFor(typer)!;
            ConnectAsCharacter(characterId);
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