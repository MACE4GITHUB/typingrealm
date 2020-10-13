using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Connections
{
    public sealed class AcknowledgingConnection : IConnection
    {
        private readonly IConnection _connection;

        public AcknowledgingConnection(IConnection connection)
        {
            _connection = connection;
        }

        public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
        {
            var message = await _connection.ReceiveAsync(cancellationToken).ConfigureAwait(false);

            if (message is Message messageWithId && messageWithId.MessageId != null)
            {
                await _connection.SendAsync(new AcknowledgeReceived
                {
                    MessageId = messageWithId.MessageId
                }, cancellationToken).ConfigureAwait(false);
            }

            return message;
        }

        public ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            return _connection.SendAsync(message, cancellationToken);
        }
    }
}
