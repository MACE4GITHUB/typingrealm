using System.Threading;

namespace TypingRealm.Messaging;

public sealed class MessageIdFactory : IMessageIdFactory
{
    private uint _messageId;

    public string CreateMessageId()
    {
        // TODO: Come up with a way to constrain this value to like 10_000 instead of 4 billions.
        return Interlocked.Increment(ref _messageId).ToString();
    }
}
