using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Connections;

public sealed class LockingConnection : IConnection
{
    private readonly IConnection _connection;
    private readonly ILock _sendLock;
    private readonly ILock _receiveLock;

    public LockingConnection(
        IConnection connection,
        ILock sendLock,
        ILock receiveLock)
    {
        if (sendLock == receiveLock)
            throw new ArgumentException("Send lock should be different from receive lock.");

        _connection = connection;
        _sendLock = sendLock;
        _receiveLock = receiveLock;
    }

    public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
    {
        await using (await _sendLock.UseWaitAsync(cancellationToken).ConfigureAwait(false))
        {
            await _connection.SendAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
    {
        await using (await _receiveLock.UseWaitAsync(cancellationToken).ConfigureAwait(false))
        {
            return await _connection.ReceiveAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
