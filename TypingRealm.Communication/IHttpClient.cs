using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public interface IHttpClient
    {
        ValueTask<T> GetAsync<T>(string uri, string? accessToken, CancellationToken cancellationToken);
        ValueTask PostAsync<T>(string uri, string? accessToken, T content, CancellationToken cancellationToken);
    }
}
