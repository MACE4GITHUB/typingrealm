using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public interface IServiceTokenService
    {
        ValueTask<string> GetServiceAccessTokenAsync(CancellationToken cancellationToken);
    }
}
