namespace TypingRealm.Messaging.Messages
{
    /// <summary>
    /// Broadcast announcement to all the clients.
    /// </summary>
    [Message]
    public sealed class Announce
    {
#pragma warning disable CS8618
        public Announce() { }
#pragma warning restore CS8618
        public Announce(string message) => Message = message;

        public string Message { get; set; }
    }
}
