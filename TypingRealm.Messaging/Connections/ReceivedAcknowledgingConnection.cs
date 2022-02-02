using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Connections;

public sealed class ReceivedAcknowledgingConnection : IConnection
{
    private readonly IConnection _connection;

    public ReceivedAcknowledgingConnection(IConnection connection)
    {
        _connection = connection;
    }

    public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
    {
        // TODO: Move this logic to ConnectionHandler so it's always executing on the server.
        var message = await _connection.ReceiveAsync(cancellationToken).ConfigureAwait(false);

        var metadata = message.GetMetadataOrEmpty();
        if (metadata.AcknowledgementType == AcknowledgementType.Received && metadata.MessageId != null)
        {
            var serverToClientMetadata = new ServerToClientMessageMetadata
            {
                MessageId = metadata.MessageId
            };

            await _connection.SendAsync(new AcknowledgeReceived(metadata.MessageId), serverToClientMetadata, cancellationToken)
                .ConfigureAwait(false);
        }

        return message;
    }

    public ValueTask SendAsync(object message, CancellationToken cancellationToken)
    {
        return _connection.SendAsync(message, cancellationToken);
    }
}
