using System.Collections.Generic;

namespace TypingRealm.Messaging;

public sealed class ClientToServerMessageMetadata : MessageMetadata
{
    public AcknowledgementType AcknowledgementType { get; set; } = AcknowledgementType.Handled;

    /// <summary>
    /// Requests response with this type from the server.
    /// </summary>
    public string? ResponseMessageTypeId { get; set; }

    // This is a hack, preferably the server should decide which groups have been affected by the client.
    public List<string>? AffectedGroups { get; set; }

    public static ClientToServerMessageMetadata CreateEmpty() => new ClientToServerMessageMetadata();
}
