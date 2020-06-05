using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages
{
    /// <summary>
    /// Combat-specific message, used to get out of combat for now.
    /// </summary>
    [Message]
    public sealed class Surrender
    {
    }
}
