using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Connections;

/// <summary>
/// Gets the actual message within <see cref="ClientToServerMessageWithMetadata"/>.
/// </summary>
// TODO: Consider moving this connection to server Messaging assembly. Only server needs it.
// TODO: Unit test this class.
public sealed class ServerMessageUnwrapperConnection : IConnection
{
    private readonly IConnection _connection;

    public ServerMessageUnwrapperConnection(IConnection connection)
    {
        _connection = connection;
    }

    public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
    {
        var message = await _connection.ReceiveAsync(cancellationToken)
            .ConfigureAwait(false);

        if (message is not MessageWithMetadata messageWithMetadata)
            throw new InvalidOperationException($"Received message is not of {typeof(MessageWithMetadata).Name} type.");

        return messageWithMetadata.Message;
    }

    public ValueTask SendAsync(object message, CancellationToken cancellationToken)
    {
        return _connection.SendAsync(message, cancellationToken);
    }
}
