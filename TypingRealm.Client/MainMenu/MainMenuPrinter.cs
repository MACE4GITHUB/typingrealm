﻿using System.Linq;
using TypingRealm.Client.Output;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuPrinter : IPrinter<MainMenuScreenState>
    {
        private readonly IOutput _output;

        public MainMenuPrinter(IOutput output)
        {
            _output = output;
        }

        public void Print(MainMenuScreenState state)
        {
            _output.WriteLine("Main menu:");
            _output.WriteLine();
            _output.Write("Exit - ");
            _output.WriteLine(state.Typers.Exit);
            _output.Write("Create new character");
            _output.Write(new string(' ', 10));
            _output.Write(state.Typers.CreateCharacter);
            _output.WriteLine();

            if (!state.Characters.Any())
                return;

            _output.WriteLine("You have these characters:");
            _output.WriteLine();

            foreach (var character in state.Characters)
            {
                var typer = state.Typers.GetSelectCharacterTyper(character.CharacterId);

                _output.Write(character.Name);
                _output.Write(new string(' ', 10));
                _output.Write(typer);
            }
        }
    }
}
