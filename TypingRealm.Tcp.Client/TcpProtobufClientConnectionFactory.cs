using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Serialization.Protobuf;

namespace TypingRealm.Tcp.Client;

public sealed class TcpProtobufClientConnectionFactory : IClientConnectionFactory
{
    private readonly IProtobufConnectionFactory _factory;
    private readonly string _host;
    private readonly int _port;

    public TcpProtobufClientConnectionFactory(
        IProtobufConnectionFactory factory,
        string host, int port)
    {
        _factory = factory;
        _host = host;
        _port = port;
    }

    public async ValueTask<ConnectionWithDisconnect> ConnectAsync(CancellationToken cancellationToken)
    {
        var client = new TcpClient();
        await client.ConnectAsync(_host, _port, cancellationToken).ConfigureAwait(false);

        var stream = client.GetStream();
        var sendLock = new SemaphoreSlimLock();
        var receiveLock = new SemaphoreSlimLock();
        var connection = _factory.CreateProtobufConnectionForClient(stream)
            .WithLocking(sendLock, receiveLock);

        return new ConnectionWithDisconnect(connection, async () =>
        {
            receiveLock.Dispose();
            sendLock.Dispose();
            stream.Close();

            await stream.DisposeAsync().ConfigureAwait(false);
            client.Dispose();
        });
    }
}
