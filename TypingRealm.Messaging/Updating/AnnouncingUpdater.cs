using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Messaging.Updating;

/// <summary>
/// Simple updater that just sends "Update" announce message to the client.
/// Used for testing purposes.
/// </summary>
public sealed class AnnouncingUpdater : IUpdater
{
    public ValueTask SendUpdateAsync(ConnectedClient client, CancellationToken cancellationToken)
    {
        return client.Connection.SendAsync(new Announce("Update"), cancellationToken);
    }
}
