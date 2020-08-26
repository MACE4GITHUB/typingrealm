using System;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.RopeWar
{
    public sealed class ContestUpdateFactory : IUpdateFactory
    {
        private readonly IContestStore _contestStore;

        public ContestUpdateFactory(IContestStore contestStore)
        {
            _contestStore = contestStore;
        }

        public object GetUpdateFor(string clientId)
        {
            var contest = _contestStore.FindByContestantId(clientId);
            if (contest == null)
                throw new InvalidOperationException($"Contest is not found for contestant {clientId}.");

            var data = contest.GetData();

            return new ContestUpdate
            {
                Progress = data.Progress,
                LeftSide = data.Contestants[Side.Left],
                RightSide = data.Contestants[Side.Right],
                HasStarted = data.HasStarted,
                HasEnded = data.HasEnded
            };
        }
    }
}
