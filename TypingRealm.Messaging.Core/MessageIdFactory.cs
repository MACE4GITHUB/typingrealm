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
    private int _resetting;
    private uint _messageId;

    public string CreateMessageId()
    {
        if (_messageId >= 10000)
        {
            var previousValue = Interlocked.CompareExchange(ref _resetting, 1, 0);
            if (previousValue == 0)
            {
                Interlocked.Exchange(ref _messageId, 0);
            }
        }
        else
        {
            if (_messageId >= 5000 && _resetting == 1)
            {
                _resetting = 0;
            }
        }

        return Interlocked.Increment(ref _messageId).ToString();
    }
}
