using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Client;

namespace TypingRealm.Client.Interaction
{
    public interface IConnectionManager
    {
        IMessageProcessor? WorldConnection { get; }
        IMessageProcessor? RopeWarConnection { get; }

        ValueTask ConnectToWorldAsync(string characterId, CancellationToken cancellationToken);
        ValueTask ConnectToRopeWarAsync(string characterId, string ropeWarContestId, CancellationToken cancellationToken);
        void DisconnectFromRopeWar();
        void DisconnectFromWorld();
    }
}
