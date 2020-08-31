using System;
using System.Linq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.RopeWar
{
    public sealed class ContestUpdateFactory : IUpdateFactory
    {
        private readonly IContestStore _contestStore;
        private readonly IConnectedClientStore _connectedClients;

        public ContestUpdateFactory(
            IContestStore contestStore,
            IConnectedClientStore connectedClients)
        {
            _contestStore = contestStore;
            _connectedClients = connectedClients;
        }

        public object GetUpdateFor(string clientId)
        {
            var contest = _contestStore.FindByContestantId(clientId);

            if (contest == null)
            {
                // This is a bit of a hack. Either pass ConnectedClient to this method,
                // or have a domain-specific ISpectatorsStore that will be filled by ConnectHook.
                var client = _connectedClients.Find(clientId);
                if (client == null)
                    throw new InvalidOperationException("Client is not connected already.");

                var contestId = client.Group;
                contest = _contestStore.Find(contestId);
                if (contest == null)
                    return new ContestUpdate(); // Not started yet.
            }

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
