using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    // TODO: Unite common logit between two HTTP client implementations.
    public sealed class AnonymousHttpClient : IHttpClient
    {
        public async ValueTask<T> GetAsync<T>(string uri, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();

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
