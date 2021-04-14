using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

namespace TypingRealm.Chat
{
    public sealed class SayHandler : IMessageHandler<Say>
    {
        private readonly MessageLog _messageLog;
        private readonly object _lock = new object();

        public SayHandler(MessageLog messageLog)
        {
            _messageLog = messageLog;
        }

        public ValueTask HandleAsync(ConnectedClient sender, Say message, CancellationToken cancellationToken)
        {
            var formattedMessage = $"{sender.ClientId} > {message.Message}";

            lock (_lock)
            {
                _messageLog.Messages.Add(formattedMessage);
            }

            return default;
        }
    }
}
