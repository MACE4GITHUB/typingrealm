using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.TypingDuels;

public sealed class ConnectionGroupProvider : IConnectionGroupProvider
{
    // TODO: Reuse smth like ConnectConnectionGroupProvider (using generic Connect message) with probably some hooks to validate groups.
    public async ValueTask<IEnumerable<string>> GetGroupsAsync(IConnection connection, CancellationToken cancellationToken)
    {
        var message = await connection.ReceiveAsync(cancellationToken)
            .ConfigureAwait(false);

        if (message is not ConnectToSession connectToSession)
            throw new InvalidOperationException("First domain message is not ConnectToSession.");

        // TODO: Validate that user belongs to session.

        return new[] { connectToSession.TypingSessionId };
    }
}
