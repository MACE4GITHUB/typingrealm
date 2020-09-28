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
                ["profile"] = "http://localhost:30103/api"
            };

        private readonly IHttpClient _httpClient;

        public InMemoryServiceClient(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public ValueTask<T> GetAsync<T>(string serviceName, string endpoint, CancellationToken cancellationToken)
        {
            if (!_serviceAddresses.ContainsKey(serviceName))
                throw new InvalidOperationException("Service is not registered in service discovery.");

            var uri = $"{_serviceAddresses[serviceName]}/{endpoint}";

            return _httpClient.GetAsync<T>(uri, cancellationToken);
        }
    }
}
