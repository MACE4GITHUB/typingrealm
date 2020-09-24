using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public interface IHttpClient
    {
        ValueTask<T> GetAsync<T>(string uri, CancellationToken cancellationToken);
    }
}
