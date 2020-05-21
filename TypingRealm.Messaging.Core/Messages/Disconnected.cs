namespace TypingRealm.Messaging.Messages
{
    /// <summary>
    /// Disconnected message indicates that the client has been disconnected
    /// from server. It is sent to client whenever disconnect happens.
    /// </summary>
    [Message]
    public sealed class Disconnected
    {
#pragma warning disable CS8618
        public Disconnected() { }
#pragma warning restore CS8618
        public Disconnected(string reason) => Reason = reason;

        /// <summary>
        /// Reason for disconnecting.
        /// </summary>
        public string Reason { get; set; }
    }
}
