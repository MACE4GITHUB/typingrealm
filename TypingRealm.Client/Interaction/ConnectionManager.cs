using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Messages;
using TypingRealm.World;

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

        public WorldState? CurrentWorldState { get; set; }

        public ValueTask ConnectToRopeWarAsync(string characterId, string ropeWarContestId, CancellationToken cancellationToken)
        {
            RopeWarConnection = _messageProcessorFactory.CreateMessageProcessorFor("rope-war");
            return RopeWarConnection.ConnectAsync(characterId, cancellationToken);
            // TODO: connect to specific contest (or refactor RopeWar domain so that the character is connected to active activity automatically).
        }

        public ValueTask ConnectToWorldAsync(string characterId, CancellationToken cancellationToken)
        {
            WorldConnection = _messageProcessorFactory.CreateMessageProcessorFor("http://127.0.0.1:30111/hub"); // world connection string.

            _ = WorldConnection.Subscribe<WorldState>(state =>
            {
                CurrentWorldState = state;
                return default;
            });

            return WorldConnection.ConnectAsync(characterId, cancellationToken);
        }

        public void DisconnectFromRopeWar()
        {
            _ = RopeWarConnection?.SendAsync(new Disconnect(), default);
            //RopeWarConnection.Disconnect();
            RopeWarConnection = null;
        }

        public void DisconnectFromWorld()
        {
            // HACK: Do not wait for it, it is a temporary hack.
            // If we wait for this - server sends Disconnected message and closes the connection, but
            // we are (probably?) waiting for AcknowledgeHandled and this thing fails.
            _ = WorldConnection?.SendAsync(new Disconnect(), default);
            //WorldConnection.Disconnect();
            WorldConnection = null;
        }
    }
}
