using System.Collections.Generic;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuScreenState
    {
        public MainMenuScreenState(MainMenuTypers typers)
        {
            Typers = typers;
            Characters = new List<CharacterInfo>();
        }

        public MainMenuTypers Typers { get; }
        public List<CharacterInfo> Characters { get; set; }
    }
}
