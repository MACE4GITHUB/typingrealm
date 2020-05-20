using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging
{
    /// <summary>
    /// Connection between server and client capable of sending and receiving
    /// messages marked with <see cref="MessageAttribute"/> attribute.
    /// </summary>
    public interface IConnection : IMessageSender, IMessageReceiver
    {
    }

    /// <summary>
    /// Connection between server and client capable of sending messages marked
    /// with <see cref="MessageAttribute"/> attribute.
    /// </summary>
    public interface IMessageSender
    {
        ValueTask SendAsync(object message, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Connection between server and client capable of receiving messages
    /// marked with <see cref="MessageAttribute"/> attribute.
    /// </summary>
    public interface IMessageReceiver
    {
        ValueTask<object> ReceiveAsync(CancellationToken cancellationToken);
    }
}
