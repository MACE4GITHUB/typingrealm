using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.TypingDuels;

public sealed class TypeMessageHandler : IMessageHandler<Typed>
{
    private readonly IConnectedClientStore _connectedClients;
    private readonly ConcurrentDictionary<string, int> _clientProgresses = new();
    private readonly TypedDebouncer _debouncer;

    public TypeMessageHandler(
        IConnectedClientStore connectedClients,
        TypedDebouncer debouncer)
    {
        _connectedClients = connectedClients;
        _debouncer = debouncer;
    }

    public async ValueTask HandleAsync(ConnectedClient sender, Typed message, CancellationToken cancellationToken)
    {
        _clientProgresses.AddOrUpdate(
            sender.ClientId,
            message.TypedCharactersCount,
            (_, _) => message.TypedCharactersCount);

        foreach (var client in _connectedClients.FindInGroups("Lobby"))
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
