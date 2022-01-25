using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.RopeWar.Handlers;

public sealed class ConnectHook : IConnectHook
{
    private readonly IContestStore _contestStore;
    private readonly ICharacterStateService _characterStateService;

    public ConnectHook(IContestStore contestStore, ICharacterStateService characterStateService)
    {
        _contestStore = contestStore;
        _characterStateService = characterStateService;
    }

    public async ValueTask HandleAsync(Connect connect, CancellationToken cancellationToken)
    {
        var contest = _contestStore.FindActiveByContestantId(connect.ClientId);
        if (contest != null)
        {
            // Already in contest.
            connect.Group = contest.ContestId;
            return;
        }

        var canJoin = await _characterStateService.CanJoinRopeWarContestAsync(connect.ClientId, connect.Group, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            if (!canJoin)
                throw new InvalidOperationException($"Cannot join contest {connect.Group}.");
        }
        catch { }
    }
}
