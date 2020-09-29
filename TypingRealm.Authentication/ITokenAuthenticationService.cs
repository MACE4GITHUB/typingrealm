using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public interface ITokenAuthenticationService
    {
        ValueTask<AuthenticationResult> AuthenticateAsync(string accessToken, CancellationToken cancellationToken);
    }
}
