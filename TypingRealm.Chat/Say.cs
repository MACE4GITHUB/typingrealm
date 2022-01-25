using TypingRealm.Messaging;

namespace TypingRealm.Chat;

[Message]
public sealed class Say
{
#pragma warning disable CS8618
    public Say() { }
#pragma warning restore CS8618
    public Say(string message)
    {
        Message = message;
    }

    public string Message { get; set; }
}
