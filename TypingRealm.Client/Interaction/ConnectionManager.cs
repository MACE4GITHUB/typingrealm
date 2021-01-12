using TypingRealm.Messaging;

namespace TypingRealm.Client.Interaction
{
    public sealed class ConnectionManager : IConnectionManager
    {
        public IConnection? WorldConnection => null;

        public IConnection? RopeWarConnection => null;

        public void ConnectToRopeWar(string ropeWarContestId)
        {
        }

        public void ConnectToWorld(string characterId)
        {
        }

        public void DisconnectFromRopeWar()
        {
        }

        public void DisconnectFromWorld()
        {
        }
    }
}
