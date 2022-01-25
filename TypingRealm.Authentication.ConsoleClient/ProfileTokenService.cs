using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Client;

namespace TypingRealm.Authentication.ConsoleClient;

public sealed class ProfileTokenProvider : IProfileTokenService
{
    private readonly IProfileTokenProvider _profileTokenProvider;

    public ProfileTokenProvider(IProfileTokenProvider profileTokenProvider)
    {
        _profileTokenProvider = profileTokenProvider;
    }

    public ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken)
    {
        return _profileTokenProvider.SignInAsync(cancellationToken);
    }
}
