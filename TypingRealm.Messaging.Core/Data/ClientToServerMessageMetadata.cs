using System.Collections.Generic;

namespace TypingRealm.Messaging;

public sealed class ClientToServerMessageMetadata : MessageMetadata
{
    // This is a hack, preferably the server should decide which groups have been affected by the client.
    public List<string>? AffectedGroups { get; set; }

    public static ClientToServerMessageMetadata CreateEmpty() => new ClientToServerMessageMetadata();
}
