using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging;

public static class MessageSenderExtensions
{
    public static ValueTask SendAsync(
        this IMessageSender messageSender,
        object message,
        MessageMetadata metadata,
        CancellationToken cancellationToken)
    {
        var messageWithMetadata = new MessageWithMetadata
        {
            Message = message,
            Metadata = metadata
        };

        return messageSender.SendAsync(messageWithMetadata, cancellationToken);
    }
}
