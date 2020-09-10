using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public sealed class AuthenticatedHttpClient : IHttpClient
    {
        private readonly IProfileTokenService _profileTokenService;

        public AuthenticatedHttpClient(IProfileTokenService profileTokenService)
        {
            _profileTokenService = profileTokenService;
        }

        public async ValueTask<T> GetAsync<T>(string uri, CancellationToken cancellationToken)
        {
            var token = await _profileTokenService.GetProfileAccessTokenAsync(cancellationToken)
                .ConfigureAwait(false);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await client.GetAsync(new Uri(uri), cancellationToken).ConfigureAwait(false);
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

            return JsonSerializer.Deserialize<T>(content, options);
        }
    }
}
