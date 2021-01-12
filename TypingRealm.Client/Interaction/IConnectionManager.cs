using TypingRealm.Messaging;

namespace TypingRealm.Client.Interaction
{
    public interface IConnectionManager
    {
        IConnection? WorldConnection { get; }
        IConnection? RopeWarConnection { get; }

        void ConnectToWorld(string characterId);
        void ConnectToRopeWar(string ropeWarContestId);
        void DisconnectFromRopeWar();
        void DisconnectFromWorld();
    }
}
