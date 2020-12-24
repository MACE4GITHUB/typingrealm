using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public interface IAccessTokenProvider
    {
        ValueTask<string> GetProfileTokenAsync(CancellationToken cancellationToken);
        ValueTask<string> GetServiceTokenAsync(CancellationToken cancellationToken);
    }
}
