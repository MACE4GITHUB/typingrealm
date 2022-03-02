using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.TypingDuels;

public sealed class TypeMessageHandler : IMessageHandler<Typed>
{
    private readonly IConnectedClientStore _connectedClients;
    private readonly Dictionary<string, ConcurrentDictionary<string, int>> _clientProgresses = new();
    private readonly TypedDebouncer _debouncer;

    public TypeMessageHandler(
        IConnectedClientStore connectedClients,
        TypedDebouncer debouncer)
    {
        _connectedClients = connectedClients;
        _debouncer = debouncer;
    }

    // HACK: Temporary hack.
    public IEnumerable<Typed> GetClientProgresses(string typingSessionId) => _clientProgresses[typingSessionId].Select(x => new Typed
    {
        ClientId = x.Key,
        TypedCharactersCount = x.Value
    });

    public async ValueTask HandleAsync(ConnectedClient sender, Typed message, CancellationToken cancellationToken)
    {
        if (!_clientProgresses.ContainsKey(sender.Group))
            _clientProgresses.Add(sender.Group, new ConcurrentDictionary<string, int>());

        _clientProgresses[sender.Group].AddOrUpdate(
            sender.ClientId,
            message.TypedCharactersCount,
            (_, _) => message.TypedCharactersCount);

        foreach (var client in _connectedClients.FindInGroups(sender.Groups))
        {
            if (client.ClientId == sender.ClientId)
                continue;

            /*await client.Connection.SendAsync(new Typed
            {
                TypedCharactersCount = message.TypedCharactersCount,
                ClientId = sender.ClientId
            }, cancellationToken)
                .ConfigureAwait(false);*/

            await _debouncer.SendAsync(client, new Typed
            {
                TypedCharactersCount = message.TypedCharactersCount,
                ClientId = sender.ClientId
            }).ConfigureAwait(false);
        }
    }
}
