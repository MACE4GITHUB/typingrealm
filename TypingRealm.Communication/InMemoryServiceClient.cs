using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public sealed class InMemoryServiceClient : IServiceClient
    {
        private readonly Dictionary<string, string> _serviceAddresses
            = new Dictionary<string, string>
            {
                ["profile"] = "http://127.0.0.1:30103"
            };

        private readonly IHttpClient _httpClient;
        private readonly IAccessTokenProvider _accessTokenProvider;

        public InMemoryServiceClient(IHttpClient httpClient, IAccessTokenProvider accessTokenProvider)
        {
            _httpClient = httpClient;
            _accessTokenProvider = accessTokenProvider;
        }

        public async ValueTask<T> GetAsync<T>(string serviceName, string endpoint, EndpointAuthenticationType endpointAuthenticationType, CancellationToken cancellationToken)
        {
            if (!_serviceAddresses.ContainsKey(serviceName))
                throw new InvalidOperationException("Service is not registered in service discovery.");

            var uri = $"{_serviceAddresses[serviceName]}/{endpoint}";

            var accessToken = endpointAuthenticationType switch
            {
                EndpointAuthenticationType.Profile => await _accessTokenProvider.GetProfileTokenAsync(cancellationToken).ConfigureAwait(false),
                EndpointAuthenticationType.Service => await _accessTokenProvider.GetServiceTokenAsync(cancellationToken).ConfigureAwait(false),
                _ => null
            };

            return await _httpClient.GetAsync<T>(uri, accessToken, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
