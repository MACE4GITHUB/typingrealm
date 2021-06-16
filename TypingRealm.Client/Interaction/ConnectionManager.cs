using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Client.Interaction
{
    public sealed class ConnectionManager : IConnectionManager
    {
        private readonly IMessageProcessorFactory _messageProcessorFactory;

        public ConnectionManager(IMessageProcessorFactory messageProcessorFactory)
        {
            _messageProcessorFactory = messageProcessorFactory;
        }

        public IMessageProcessor? WorldConnection { get; private set; }
        public IMessageProcessor? RopeWarConnection { get; private set; }

        public ValueTask ConnectToRopeWarAsync(string characterId, string ropeWarContestId, CancellationToken cancellationToken)
        {
            RopeWarConnection = _messageProcessorFactory.CreateMessageProcessorFor("rope-war");
            return RopeWarConnection.ConnectAsync(characterId, cancellationToken);
            // TODO: connect to specific contest (or refactor RopeWar domain so that the character is connected to active activity automatically).
        }

        public ValueTask ConnectToWorldAsync(string characterId, CancellationToken cancellationToken)
        {
            WorldConnection = _messageProcessorFactory.CreateMessageProcessorFor("http://127.0.0.1:30111/hub"); // world connection string.
            return WorldConnection.ConnectAsync(characterId, cancellationToken);
        }

        public void DisconnectFromRopeWar()
        {
            RopeWarConnection?.SendAsync(new Disconnect(), default);
            //RopeWarConnection.Disconnect();
            RopeWarConnection = null;
        }

        public void DisconnectFromWorld()
        {
            WorldConnection?.SendAsync(new Disconnect(), default);
            //WorldConnection.Disconnect();
            WorldConnection = null;
        }
    }
}
