using TypingRealm.Messaging.Updating;

namespace TypingRealm.Chat
{
    public sealed class StateFactory : IUpdateFactory
    {
        private readonly MessageLog _messageLog;

        public StateFactory(MessageLog messageLog)
        {
            _messageLog = messageLog;
        }

        public object GetUpdateFor(string clientId)
        {
            return _messageLog;
        }
    }
}
