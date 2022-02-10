using System.Collections.Generic;

namespace TypingRealm.Messaging;

public sealed class ClientToServerMessageMetadata : MessageMetadata
{
    // TODO: Test that this works, because protobuf might skip all the derived properties here.
    // This is a hack, preferably the server should decide which groups have been affected by the client.
#pragma warning disable CA2227 // Collection properties should be read only
    public ICollection<string>? AffectedGroups { get; set; }
#pragma warning restore CA2227

    public static new ClientToServerMessageMetadata CreateEmpty() => new();
}
