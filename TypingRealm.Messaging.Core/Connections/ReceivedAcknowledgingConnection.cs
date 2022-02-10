using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Messaging.Connections;

// Used by server-side ConnectionHandler.
public sealed class ReceivedAcknowledgingConnection : IConnection
{
    private readonly IConnection _connection;

    public ReceivedAcknowledgingConnection(IConnection connection)
    {
        _connection = connection;
    }

    public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
    {
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
