using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Messages;
using TypingRealm.World;

namespace TypingRealm.Client.Interaction;

public sealed class ConnectionManager : IConnectionManager
{
    private string? _worldStateSubscription;
    private readonly IMessageProcessorFactory _messageProcessorFactory;
    private readonly BehaviorSubject<IMessageProcessor?> _worldConnectionSubject
        = new BehaviorSubject<IMessageProcessor?>(null);

    private readonly BehaviorSubject<WorldState?> _worldStateSubject
        = new BehaviorSubject<WorldState?>(null);

    public ConnectionManager(IMessageProcessorFactory messageProcessorFactory)
    {
        _messageProcessorFactory = messageProcessorFactory;
    }

    public IMessageProcessor? WorldConnection { get; private set; }
    public IObservable<IMessageProcessor?> WorldConnectionObservable => _worldConnectionSubject;
    public IObservable<WorldState?> WorldStateObservable => _worldStateSubject.AsObservable();

    public string CharacterId { get; private set; } = string.Empty;

    public async ValueTask ConnectToWorldAsync(string characterId, CancellationToken cancellationToken)
    {
        CharacterId = characterId;
        WorldConnection = _messageProcessorFactory.CreateMessageProcessorFor("http://127.0.0.1:30111/hub"); // world connection string.

        await WorldConnection.ConnectAsync(characterId, cancellationToken)
            .ConfigureAwait(false);

        _worldConnectionSubject.OnNext(WorldConnection);

        _worldStateSubscription = WorldConnection.Subscribe<WorldState>(message =>
        {
            _worldStateSubject.OnNext(message);
            return default;
        });
    }

    public void DisconnectFromWorld()
    {
        // HACK: Do not wait for it, it is a temporary hack.
        // If we wait for this - server sends Disconnected message and closes the connection, but
        // we are (probably?) waiting for AcknowledgeHandled and this thing fails.
        _ = WorldConnection?.SendAsync(new Disconnect(), default);
        //WorldConnection.Disconnect();

        if (_worldStateSubscription != null)
            WorldConnection?.Unsubscribe(_worldStateSubscription);

        WorldConnection = null;

        _worldConnectionSubject.OnNext(null);
        _worldStateSubject.OnNext(null);
    }
}
