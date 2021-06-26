using System.Collections.Generic;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuScreenState
    {
        public MainMenuScreenState(MainMenuModel typers)
        {
            Model = typers;
            Characters = new List<CharacterInfo>();
        }

        public MainMenuModel Model { get; }
        public List<CharacterInfo> Characters { get; set; }
    }
}
