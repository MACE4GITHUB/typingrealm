using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

public sealed class TypedDebouncer
{
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(100);
    private readonly ConcurrentDictionary<ConnectedClient, Typed> _scheduledMessages
        = new ConcurrentDictionary<ConnectedClient, Typed>();
    private Task _sendingProcess = Task.CompletedTask;
    private int _isWaiting = 0;

    public async ValueTask SendAsync(ConnectedClient to, Typed message)
    {
        _scheduledMessages.AddOrUpdate(to, message, (_, _) => message);

        var previous = Interlocked.CompareExchange(ref _isWaiting, 1, 0);
        if (previous == 1)
            return;

        _sendingProcess = Task.Run(async () =>
        {
            await Task.Delay(_debounceInterval)
                .ConfigureAwait(false);

            previous = Interlocked.CompareExchange(ref _isWaiting, 0, 1);
            if (previous == 0)
                return;

            foreach (var key in _scheduledMessages.Keys.ToList())
            {
                if (_scheduledMessages.TryRemove(key, out var message))
                {
                    await key.Connection.SendAsync(message, default)
                        .ConfigureAwait(false);
                }
            }
        });
    }
}

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
        builder.Services.AddSingleton<TypedDebouncer>();
        return builder;
    }
}
