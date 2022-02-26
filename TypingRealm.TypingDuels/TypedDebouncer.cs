using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging;

namespace TypingRealm.TypingDuels;

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
