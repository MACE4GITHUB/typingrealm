using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public interface IProfileTokenService
    {
        ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken);
    }
}
