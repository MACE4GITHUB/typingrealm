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

        public void ConnectToRopeWar(string ropeWarContestId)
        {
            RopeWarConnection = _messageProcessorFactory.CreateMessageProcessorFor("rope-war");
        }

        public void ConnectToWorld(string characterId)
        {
            WorldConnection = _messageProcessorFactory.CreateMessageProcessorFor("world");
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
