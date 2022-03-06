using System.Threading.Tasks;

namespace TypingRealm.Messaging.Connecting;

public interface IConnectedClientInitializer
{
    ValueTask InitializeAsync(ConnectedClient connectedClient);
}
