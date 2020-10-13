namespace TypingRealm.Messaging
{
    [Message]
    public abstract class Message
    {
        /// <summary>
        /// Message identifier should be set to unique value. It is used for
        /// idempotency support and for the server to reply with
        /// <see cref="AcknowledgeReceived"/> message to simulate RPC calls.
        /// Leave it NULL to leave the message completely asynchronous.
        /// </summary>
        public string? MessageId { get; set; }
    }
}
