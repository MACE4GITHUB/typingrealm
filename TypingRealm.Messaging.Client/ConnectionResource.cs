using System;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Client
{
    public sealed class ConnectionResource
    {
        private readonly Func<ValueTask> _disconnect;

        public ConnectionResource(IConnection connection, Func<ValueTask> disconnect)
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
