using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

namespace TypingRealm.RopeWar.Handlers;

public sealed class JoinContestHandler : IMessageHandler<JoinContest>
{
    private readonly IContestStore _contestStore;

    public JoinContestHandler(IContestStore contestStore)
    {
        _contestStore = contestStore;
    }

    public ValueTask HandleAsync(ConnectedClient sender, JoinContest message, CancellationToken cancellationToken)
    {
        var existingContest = _contestStore.FindActiveByContestantId(sender.ClientId);
        if (existingContest != null)
            throw new InvalidOperationException($"Already in contest {existingContest.ContestId}");

        var contest = _contestStore.Find(sender.Group);
        if (contest == null)
            contest = new Contest(sender.Group);

        contest.Join(sender.ClientId, message.Side);
        _contestStore.Save(contest);

        return default;
    }
}
