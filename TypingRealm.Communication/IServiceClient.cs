using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public interface IServiceClient
    {
        ValueTask<T> GetAsync<T>(
            string serviceName,
            string endpoint,
            EndpointAuthenticationType endpointAuthenticationType,
            CancellationToken cancellationToken);

        ValueTask PostAsync<T>(
            string serviceName,
            string endpoint,
            EndpointAuthenticationType endpointAuthenticationType,
            T content,
            CancellationToken cancellationToken);
    }
}
