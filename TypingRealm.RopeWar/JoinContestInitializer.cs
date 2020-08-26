using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.RopeWar
{
    public sealed class JoinContestInitializer : IConnectionInitializer
    {
        private readonly IContestStore _contestStore;
        private readonly IUpdateDetector _updateDetector;

        public JoinContestInitializer(
            IContestStore contestStore,
            IUpdateDetector updateDetector)
        {
            _contestStore = contestStore;
            _updateDetector = updateDetector;
        }

        public async ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
        {
            if (!(await connection.ReceiveAsync(cancellationToken).ConfigureAwait(false) is JoinContest joinContest))
                throw new InvalidOperationException("First message is not a JoinContest message.");

            // TODO: This should be locked (on the level of messaging framework connection initializers).
            var contest = _contestStore.Find(joinContest.ContestId);
            if (contest == null)
                contest = new Contest(joinContest.ContestId);

            contest.Join(joinContest.ContestantId, joinContest.Side);
            _contestStore.Save(contest);

            return new ConnectedClient(joinContest.ContestantId, connection, joinContest.ContestId, _updateDetector);
        }
    }
}
