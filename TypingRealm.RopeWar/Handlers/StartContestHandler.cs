using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

namespace TypingRealm.RopeWar.Handlers
{
    public sealed class StartContestHandler : IMessageHandler<StartContest>
    {
        private readonly IContestStore _contestStore;

        public StartContestHandler(IContestStore contestStore)
        {
            _contestStore = contestStore;
        }

        public ValueTask HandleAsync(ConnectedClient sender, StartContest message, CancellationToken cancellationToken)
        {
            var contestantId = sender.ClientId;

            var contest = _contestStore.FindActiveByContestantId(contestantId);
            if (contest == null)
                throw new InvalidOperationException($"Contest is not found for contestant {contestantId}.");

            contest.Start();
            _contestStore.Save(contest);

            return default;
        }
    }
}
