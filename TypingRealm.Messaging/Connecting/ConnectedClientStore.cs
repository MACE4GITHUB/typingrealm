using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging.Connecting
{
    public sealed class ConnectedClientStore : IConnectedClientStore
    {
        private readonly ConcurrentDictionary<string, ConnectedClient> _connectedClients
            = new ConcurrentDictionary<string, ConnectedClient>();
        private readonly IUpdateDetector _updateDetector;

        public ConnectedClientStore(IUpdateDetector updateDetector)
        {
            _updateDetector = updateDetector;
        }

        public void Add(ConnectedClient connectedClient)
        {
            if (!_connectedClients.TryAdd(connectedClient.ClientId, connectedClient))
                throw new ClientAlreadyConnectedException(connectedClient);

            _updateDetector.MarkForUpdate(connectedClient.Group);
        }

        public ConnectedClient? Find(string clientId)
        {
            _connectedClients.TryGetValue(clientId, out var client);
            return client;
        }

        public IEnumerable<ConnectedClient> FindInGroups(IEnumerable<string> groups)
        {
            return _connectedClients.Values.Where(x => groups.Contains(x.Group)).ToList();
        }

        public void Remove(string clientId)
        {
            if (_connectedClients.TryRemove(clientId, out var removed))
                _updateDetector.MarkForUpdate(removed.Group);
        }
    }
}
