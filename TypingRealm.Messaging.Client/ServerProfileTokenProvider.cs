using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication;

namespace TypingRealm.Messaging.Client;

/// <summary>
/// Profile token provider that gets the token from the authenticated
/// connection client context <see cref="IProfileTokenService"/>.
/// </summary>
public sealed class ServerProfileTokenProvider : IProfileTokenProvider
{
    private readonly IProfileTokenService _profileTokenService;

    public ServerProfileTokenProvider(IProfileTokenService profileTokenService)
    {
        _profileTokenService = profileTokenService;
    }

    public ValueTask<string> SignInAsync(CancellationToken cancellationToken)
    {
        return _profileTokenService.GetProfileAccessTokenAsync(cancellationToken);
    }
}
