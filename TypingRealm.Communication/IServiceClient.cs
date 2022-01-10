using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication;

namespace TypingRealm.Communication
{
    public interface IServiceClient
    {
        ValueTask<T> GetAsync<T>(
            string serviceName,
            string endpoint,
            EndpointAuthentication endpointAuthentication,
            CancellationToken cancellationToken);

        ValueTask PostAsync<T>(
            string serviceName,
            string endpoint,
            EndpointAuthentication endpointAuthentication,
            T content,
            CancellationToken cancellationToken);

        ValueTask<TResponse> PostAsync<TBody, TResponse>(
            string serviceName,
            string endpoint,
            EndpointAuthentication endpointAuthentication,
            TBody content,
            CancellationToken cancellationToken);

        ValueTask DeleteAsync(
            string serviceName,
            string endpoint,
            EndpointAuthentication endpointAuthentication,
            CancellationToken cancellationToken);
    }

    public sealed class EndpointAuthentication
    {
        private EndpointAuthentication(EndpointAuthenticationType authenticationType)
        {
            AuthenticationType = authenticationType;
        }

        private EndpointAuthentication(ClientCredentials credentials)
        {
            AuthenticationType = EndpointAuthenticationType.Service;
            Credentials = credentials;
        }

        public EndpointAuthenticationType AuthenticationType { get; }
        internal ClientCredentials? Credentials { get; }

        public static EndpointAuthentication Anonymous => new(EndpointAuthenticationType.Anonymous);
        public static EndpointAuthentication Service => new(EndpointAuthenticationType.Service);
        public static EndpointAuthentication Profile => new(EndpointAuthenticationType.Profile);

        public static EndpointAuthentication FromClientCredentials(ClientCredentials credentials)
            => new(credentials);
    }
}
