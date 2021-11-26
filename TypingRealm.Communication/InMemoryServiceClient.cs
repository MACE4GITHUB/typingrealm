using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public sealed class InMemoryServiceClient : IServiceClient
    {
        // HACK: Make sure when proper configuration is implemented we are not getting environment variables here.
        // And also in IdentityServerAuthenticationConfiguration.

        private readonly Dictionary<string, string> _serviceAddresses
            = new Dictionary<string, string>
            {
                ["profiles"] = GetProfilesHost(),
                ["data"] = GetDataHost()
            };

        private static string GetProfilesHost()
        {
            var profilesHost = Environment.GetEnvironmentVariable("PROFILES_URL");
            if (profilesHost == null)
                return "http://127.0.0.1:30103";

            return profilesHost;
        }

        private static string GetDataHost()
        {
            var dataHost = Environment.GetEnvironmentVariable("DATA_URL");
            if (dataHost == null)
                return "http://127.0.0.1:30400";

            return dataHost;
        }

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

            try
            {
                return await _httpClient.GetAsync<T>(uri, accessToken, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                // TODO: Consider placing it in some other place of code, like IHttpClient.
                // TODO: Fix the issue that if endpoint return STRUCT - we return default value when it's not found!
                if (exception.StatusCode == HttpStatusCode.NotFound)
                    return default!;

                throw;
            }
        }

        public async ValueTask PostAsync<T>(string serviceName, string endpoint, EndpointAuthenticationType endpointAuthenticationType, T content, CancellationToken cancellationToken)
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

            await _httpClient.PostAsync(uri, accessToken, content, cancellationToken)
                .ConfigureAwait(false);
        }

        public async ValueTask DeleteAsync(string serviceName, string endpoint, EndpointAuthenticationType endpointAuthenticationType, CancellationToken cancellationToken)
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

            await _httpClient.DeleteAsync(uri, accessToken, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
