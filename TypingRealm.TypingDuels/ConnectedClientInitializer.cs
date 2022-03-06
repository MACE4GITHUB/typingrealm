using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.TypingDuels;

public sealed class ConnectedClientInitializer : IConnectedClientInitializer
{
    private readonly TypingDuelsState _state;

    public ConnectedClientInitializer(TypingDuelsState state)
    {
        _state = state;
    }

    public ValueTask InitializeAsync(ConnectedClient connectedClient)
    {
        _state.InitializeProgress(connectedClient.Group, connectedClient.ClientId);
        return default;
    }
}
