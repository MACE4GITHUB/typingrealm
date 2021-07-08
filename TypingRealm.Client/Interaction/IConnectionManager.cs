using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Client;

namespace TypingRealm.Client.Interaction
{
    public interface IConnectionManager
    {
        IMessageProcessor? WorldConnection { get; }
        IObservable<IMessageProcessor?> WorldConnectionObservable { get; }

        ValueTask ConnectToWorldAsync(string characterId, CancellationToken cancellationToken);
        void DisconnectFromWorld();

        string CharacterId { get; }
    }
}
