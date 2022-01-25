using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Domain;

/// <summary>
/// Accepts only those connections whose first message is a valid
/// <see cref="Join"/> message with supplied PlayerId.
/// </summary>
public sealed class ConnectionInitializer : IConnectionInitializer
{
    private readonly IUpdateDetector _updateDetector;
    private readonly IPlayerRepository _playerRepository;

    public ConnectionInitializer(
        IUpdateDetector updateDetector,
        IPlayerRepository playerRepository)
    {
        _updateDetector = updateDetector;
        _playerRepository = playerRepository;
    }

    public async ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
    {
        if (!(await connection.ReceiveAsync(cancellationToken).ConfigureAwait(false) is Join join))
            throw new InvalidOperationException($"Could not connect: first message is not a valid {nameof(Join)} message.");

        var playerId = new PlayerId(join.PlayerId);
        var player = _playerRepository.Find(playerId);
        if (player == null)
            throw new InvalidOperationException($"Could not connect: could not find player {playerId}.");

        var group = player.MessagingGroup;
        return new ConnectedClient(playerId, connection, _updateDetector, group);
    }
}
