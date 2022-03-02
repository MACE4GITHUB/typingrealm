using System.Linq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.TypingDuels;

public sealed class UpdateFactory : IUpdateFactory
{
    // HACK: Temporary hack.
    private readonly TypeMessageHandler _handler;
    private readonly IConnectedClientStore _connectedClients;

    public UpdateFactory(
        TypeMessageHandler handler,
        IConnectedClientStore connectedClients)
    {
        _handler = handler;
        _connectedClients = connectedClients;
    }

    public object GetUpdateFor(string clientId)
    {
        var client = _connectedClients.Find(clientId);
        var clients = _connectedClients.FindInGroups(client!.Groups);

        var progresses = _handler.GetClientProgresses(client.Group).Where(c => c.ClientId != clientId && clients.Select(x => x.ClientId).Contains(c.ClientId));
        return new Update
        {
            Progresses = progresses.ToList()
        };
    }
}
