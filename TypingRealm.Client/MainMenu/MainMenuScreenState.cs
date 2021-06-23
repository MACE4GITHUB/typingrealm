using System.Collections.Generic;

namespace TypingRealm.Client.MainMenu
{
    public sealed class MainMenuScreenState
    {
        public MainMenuScreenState()
        {
            Characters = new List<CharacterInfo>();
        }

        public List<CharacterInfo> Characters { get; set; }
    }
}
