using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging
{
    public static class MessageSenderExtensions
    {
        public static ValueTask SendAsync(
            this IMessageSender messageSender,
            object message,
            ClientToServerMessageMetadata metadata,
            CancellationToken cancellationToken)
        {
            var messageWithMetadata = new ClientToServerMessageWithMetadata
            {
                Message = message,
                Metadata = metadata
            };

            return messageSender.SendAsync(messageWithMetadata, cancellationToken);
        }

        public static ValueTask SendAsync(
            this IMessageSender messageSender,
            object message,
            ServerToClientMessageMetadata metadata,
            CancellationToken cancellationToken)
        {
            var messageWithMetadata = new ServerToClientMessageWithMetadata
            {
                Message = message,
                Metadata = metadata
            };

            return messageSender.SendAsync(messageWithMetadata, cancellationToken);
        }
    }
}
