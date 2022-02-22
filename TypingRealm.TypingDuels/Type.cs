using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.TypingDuels;

[Message]
public sealed class Typed
{
    public int TypedCharactersCount { get; set; }
    public string? ClientId { get; set; }
}

// Consider debouncing per client, so that unrelated clients can get their updates and load is evened out.
public sealed class TypedDebouncer
{
    private readonly ILogger<TypedDebouncer> _logger;
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(100);
    private readonly ConcurrentDictionary<ConnectedClient, Typed> _scheduledMessages
        = new ConcurrentDictionary<ConnectedClient, Typed>();
    private Task _sendingProcess = Task.CompletedTask;
    private int _isWaiting = 0;

    public TypedDebouncer(ILogger<TypedDebouncer> logger)
    {
        _logger = logger;
    }

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

            var tasks = _scheduledMessages.Keys.Select(key => Task.Run(async () =>
            {
                if (_scheduledMessages.TryRemove(key, out var message))
                {
                    try
                    {
                        await key.Connection.SendAsync(message, default)
                            .ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError($"Could not send Typed update to client {key}");
                        // TODO: Notify the framework that there are problems with this client -> ask the client to reconnect.
                    }
                }
            })).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);
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
        builder.Services.UseUpdateFactory<UpdateFactory>();
        return builder;
    }
}

#pragma warning disable CS8618
[Message]
public sealed class Update
{
}
#pragma warning restore CS8618

public sealed class UpdateFactory : IUpdateFactory
{
    public object GetUpdateFor(string clientId)
    {
        return new Update();
    }
}
