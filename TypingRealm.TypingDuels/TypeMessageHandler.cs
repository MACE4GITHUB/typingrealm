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
    private readonly TypedDebouncer _debouncer;
    private readonly TypingDuelsState _state;

    public TypeMessageHandler(
        TypingDuelsState state,
        IConnectedClientStore connectedClients,
        TypedDebouncer debouncer)
    {
        _state = state;
        _connectedClients = connectedClients;
        _debouncer = debouncer;
    }

    // HACK: Temporary hack.
    public IEnumerable<Typed> GetClientProgresses(string typingSessionId) => _state.GetProgressesForSession(typingSessionId).Select(x => new Typed
    {
        ClientId = x.Key,
        TypedCharactersCount = x.Value
    });

    public async ValueTask HandleAsync(ConnectedClient sender, Typed message, CancellationToken cancellationToken)
    {
        _state.UpdateProgress(sender.Group, sender.ClientId, message.TypedCharactersCount);

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
