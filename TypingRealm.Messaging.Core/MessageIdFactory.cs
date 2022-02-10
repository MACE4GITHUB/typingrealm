using System.Threading;

namespace TypingRealm.Messaging;

/// <summary>
/// Used to create a unique message identifier before sending it to the second party.
/// For now it's only used by client side, but it can be a universal interface.
/// </summary>
public interface IMessageIdFactory
{
    string CreateMessageId();
}

public sealed class MessageIdFactory : IMessageIdFactory
{
    private uint _messageId;

    public string CreateMessageId()
    {
        // TODO: Come up with a way to constrain this value to like 10_000 instead of 4 billions.
        return Interlocked.Increment(ref _messageId).ToString();
    }
}
