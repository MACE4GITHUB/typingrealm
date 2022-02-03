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
        // Or make sure server always uses this connection (probably as well as client, this is universal now).
        var message = await _connection.ReceiveAsync(cancellationToken).ConfigureAwait(false);
        if (message is MessageWithMetadata mwm &&
            (mwm.Message is AcknowledgeReceived || mwm.Message is AcknowledgeHandled))
            return message;

        var metadata = message.GetMetadataOrEmpty();
        if (metadata.AcknowledgementType == AcknowledgementType.Received && metadata.MessageId != null)
        {
            var ackMetadata = new MessageMetadata
            {
                MessageId = metadata.MessageId
            };

            await _connection.SendAsync(new AcknowledgeReceived(metadata.MessageId), ackMetadata, cancellationToken)
                .ConfigureAwait(false);
        }

        return message;
    }

    public ValueTask SendAsync(object message, CancellationToken cancellationToken)
    {
        return _connection.SendAsync(message, cancellationToken);
    }
}
