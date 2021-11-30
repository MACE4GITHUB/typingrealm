using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    /// <summary>
    /// Use this interface to get user's profile token.
    ///
    /// It gets profile token from the ConnectedClient for real time server,
    /// and from HttpClient for Web API.
    ///
    /// If HttpClient has service token, it returns service token (not NULL).
    /// To get a new service token, use <see cref="IServiceTokenService"/>.
    /// </summary>
    public interface IProfileTokenService
    {
        ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken);
    }
}
