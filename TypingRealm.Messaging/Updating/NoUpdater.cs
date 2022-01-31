using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Updating;

/// <summary>
/// By default do not send any updates, performance optimization.
/// </summary>
public sealed class NoUpdater : IUpdater
{
    public ValueTask SendUpdateAsync(ConnectedClient client, CancellationToken cancellationToken)
    {
        return default;
    }
}
