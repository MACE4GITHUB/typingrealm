using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TypingRealm.Authentication.Api;
using TypingRealm.Authentication.Service;

namespace TypingRealm.Authentication;

public sealed class HttpOrConnectedClientProfileTokenService : IProfileTokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConnectedClientContext _connectedClientContext;

    public HttpOrConnectedClientProfileTokenService(
        IHttpContextAccessor httpContextAccessor,
        IConnectedClientContext connectedClientContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _connectedClientContext = connectedClientContext;
    }

    public async ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken)
    {
        var service = new HttpContextProfileTokenService(_httpContextAccessor);
        try
        {
            return await service.GetProfileAccessTokenAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            // TODO: Log exception.
            var service2 = new ConnectedClientProfileTokenService(_connectedClientContext);
            return await service2.GetProfileAccessTokenAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
