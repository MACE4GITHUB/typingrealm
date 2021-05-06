using System;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Client
{
    public sealed class ConnectionWithDisconnect
    {
        private readonly Func<ValueTask> _disconnect;

        public ConnectionWithDisconnect(IConnection connection, Func<ValueTask> disconnect)
        {
            Connection = connection;
            _disconnect = disconnect;
        }

        public IConnection Connection { get; }

        public ValueTask DisconnectAsync()
        {
            return _disconnect();
        }
    }
}
