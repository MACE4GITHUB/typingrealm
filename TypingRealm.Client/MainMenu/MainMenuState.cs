using System.Collections.Generic;
using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Client.MainMenu
{
    public sealed record MainMenuState(IEnumerable<CharacterResource> Characters);
}
