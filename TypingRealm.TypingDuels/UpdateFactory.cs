using System.Linq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.TypingDuels;

public sealed class UpdateFactory : IUpdateFactory
{
    private readonly TypingDuelsState _state;
    private readonly IConnectedClientStore _connectedClients;

    public UpdateFactory(
        TypingDuelsState state,
        IConnectedClientStore connectedClients)
    {
        _state = state;
        _connectedClients = connectedClients;
    }

    public object GetUpdateFor(string clientId)
    {
        var client = _connectedClients.Find(clientId);
        var clients = _connectedClients.FindInGroups(client!.Groups);

        var progresses = _state.GetProgressesForSession(client.Group)
            .Where(c => c.Key != clientId && clients.Select(x => x.ClientId).Contains(c.Key))
            .Select(x => new Typed
            {
                ClientId = x.Key,
                TypedCharactersCount = x.Value
            });

        return new Update
        {
            Progresses = progresses.ToList()
        };
    }
}
