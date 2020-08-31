using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

namespace TypingRealm.RopeWar.Handlers
{
    public sealed class PullRopeHandler : IMessageHandler<PullRope>
    {
        private readonly IContestStore _contestStore;

        public PullRopeHandler(IContestStore contestStore)
        {
            _contestStore = contestStore;
        }

        public ValueTask HandleAsync(ConnectedClient sender, PullRope message, CancellationToken cancellationToken)
        {
            var contestantId = sender.ClientId;

            var contest = _contestStore.FindActiveByContestantId(contestantId);
            if (contest == null)
                throw new InvalidOperationException($"Contest is not found for contestant {contestantId}.");

            contest.PullRope(contestantId, message.Distance);
            _contestStore.Save(contest);

            return default;
        }
    }
}
