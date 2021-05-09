using System.Collections.Generic;

namespace TypingRealm.Messaging
{
    public sealed class ClientToServerMessageMetadata
    {
        public string? MessageId { get; set; }
        public bool RequireAcknowledgement { get; set; } = true;

        /// <summary>
        /// Requests response with this type from the server.
        /// </summary>
        public string? ResponseMessageTypeId { get; set; }

        public List<string>? AffectedGroups { get; set; }

        public static ClientToServerMessageMetadata CreateEmpty() => new ClientToServerMessageMetadata();

        public void EnableAcknowledgement(string messageId)
        {
            MessageId = messageId;
            RequireAcknowledgement = true;
        }
    }
}
