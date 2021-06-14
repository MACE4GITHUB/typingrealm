using TypingRealm.Messaging.Client;

namespace TypingRealm.Client.Interaction
{
    public interface IConnectionManager
    {
        IMessageProcessor? WorldConnection { get; }
        IMessageProcessor? RopeWarConnection { get; }

        void ConnectToWorld(string characterId);
        void ConnectToRopeWar(string ropeWarContestId);
        void DisconnectFromRopeWar();
        void DisconnectFromWorld();
    }
}
