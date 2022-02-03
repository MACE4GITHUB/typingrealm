using System.Collections.Generic;

namespace TypingRealm.Messaging;

public sealed class ClientToServerMessageMetadata : MessageMetadata
{
    // TODO: Test that this works, because protobuf might skip all the derived properties here.
    // This is a hack, preferably the server should decide which groups have been affected by the client.
    public List<string>? AffectedGroups { get; set; }

    public static ClientToServerMessageMetadata CreateEmpty() => new ClientToServerMessageMetadata();
}
