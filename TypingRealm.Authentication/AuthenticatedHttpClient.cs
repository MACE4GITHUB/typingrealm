using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Authentication
{
    public sealed class AuthenticatedHttpClient : SyncManagedDisposable, IHttpClient
    {
        private readonly IProfileTokenService _profileTokenService;
        private readonly HttpClient _httpClient = new HttpClient();

        public AuthenticatedHttpClient(IProfileTokenService profileTokenService)
        {
            _profileTokenService = profileTokenService;
        }

        public async ValueTask<T> GetAsync<T>(string uri, CancellationToken cancellationToken)
        {
            // TODO: Consider updating token only when it's updated in ConnectedClientContext.
            var token = await _profileTokenService.GetProfileAccessTokenAsync(cancellationToken)
                .ConfigureAwait(false);

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.GetAsync(new Uri(uri), cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                return (T)Convert.ChangeType(content, typeof(T));

            // TODO: Use serialization from Messaging.Serialization.Core, don't duplicate code.
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new JsonStringEnumConverter());

            var deserialized = JsonSerializer.Deserialize<T>(content, options);
            if (deserialized == null)
                throw new InvalidOperationException("Could not deserialize response from API.");

            return deserialized;
        }

        protected override void DisposeManagedResources()
        {
            _httpClient.Dispose();
        }
    }
}
