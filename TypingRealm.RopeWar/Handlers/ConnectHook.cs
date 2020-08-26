using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.RopeWar.Handlers
{
    public sealed class ConnectHook : IConnectHook
    {
        private readonly IContestStore _contestStore;

        public ConnectHook(IContestStore contestStore)
        {
            _contestStore = contestStore;
        }

        public ValueTask HandleAsync(Connect connect, CancellationToken cancellationToken)
        {
            var contest = _contestStore.FindByContestantId(connect.ClientId);
            if (contest == null)
                return default;

            connect.Group = contest.ContestId;
            return default;
        }
    }
}
