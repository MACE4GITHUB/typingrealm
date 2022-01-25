using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Domain.Movement;

public sealed class TeleportPlayerToLocationHandler : IMessageHandler<TeleportPlayerToLocation>
{
    private readonly IPlayerRepository _players;

    public TeleportPlayerToLocationHandler(IPlayerRepository players)
    {
        _players = players;
    }

    public ValueTask HandleAsync(ConnectedClient sender, TeleportPlayerToLocation message, CancellationToken cancellationToken)
    {
        var playerId = new PlayerId(message.PlayerId);
        var locationId = new LocationId(message.LocationId);

        var player = _players.Find(playerId);
        if (player == null)
            throw new InvalidOperationException("Player does not exist.");

        player.TeleportToLocation(locationId);

        _players.Save(player);
        return default;
    }
}
