using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public interface IHttpClient
    {
        ValueTask<T> GetAsync<T>(string uri, CancellationToken cancellationToken);
    }
}
