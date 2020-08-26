using System;
using System.Linq;
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

            // TODO: Return spectator update here, do not throw an exception. The client connected but did not join the contest.
            if (contest == null)
                throw new InvalidOperationException($"Contest is not found for contestant {clientId}.");

            var data = contest.GetData();

            return new ContestUpdate
            {
                Progress = data.Progress,
                Contestants = data.Contestants.SelectMany(
                    x => x.Value.Select(
                        contestantId => new ContestantUpdate
                        {
                            ContestantId = contestantId,
                            Side = x.Key
                        })).ToList(),
                HasStarted = data.HasStarted,
                HasEnded = data.HasEnded
            };
        }
    }
}
