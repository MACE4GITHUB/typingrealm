using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication.Service;

public sealed class ConnectedClientProfileTokenService : IProfileTokenService
{
    private readonly IConnectedClientContext _connectedClientContext;

    public ConnectedClientProfileTokenService(IConnectedClientContext connectedClientContext)
    {
        _connectedClientContext = connectedClientContext;
    }

    public ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<string>(_connectedClientContext.GetAccessToken());
    }
}
