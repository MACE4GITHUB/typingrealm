using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.TypingDuels;

[Message]
public sealed class Typed
{
    public int TypedCharactersCount { get; set; }
    public string? ClientId { get; set; }
}

public sealed class TypeMessageHandler : IMessageHandler<Typed>
{
    private readonly IConnectedClientStore _connectedClients;
    private readonly ConcurrentDictionary<string, int> _clientProgresses = new();

    public TypeMessageHandler(IConnectedClientStore connectedClients)
    {
        _connectedClients = connectedClients;
    }

    public async ValueTask HandleAsync(ConnectedClient sender, Typed message, CancellationToken cancellationToken)
    {
        _clientProgresses.AddOrUpdate(
            sender.ClientId,
            message.TypedCharactersCount,
            (_, _) => message.TypedCharactersCount);

        foreach (var client in _connectedClients.FindInGroups("Lobby"))
        {
            await client.Connection.SendAsync(new Typed
            {
                TypedCharactersCount = message.TypedCharactersCount,
                ClientId = sender.ClientId
            }, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

public static class RegistrationExtensions
{
    public static MessageTypeCacheBuilder AddTypingDuelsMessages(this MessageTypeCacheBuilder builder)
    {
        builder.AddMessageTypesFromAssembly(typeof(Typed).Assembly);
        return builder;
    }

    public static MessageTypeCacheBuilder AddTypingDuelsDomain(this MessageTypeCacheBuilder builder)
    {
        builder.AddTypingDuelsMessages();
        builder.Services.RegisterHandler<Typed, TypeMessageHandler>();
        return builder;
    }
}
