using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public sealed class HttpClient : SyncManagedDisposable, IHttpClient
    {
        private readonly System.Net.Http.HttpClient _httpCLient = new System.Net.Http.HttpClient();

        public async ValueTask<T> GetAsync<T>(string uri, string accessToken, CancellationToken cancellationToken)
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, uri);

            if (accessToken != null)
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await _httpCLient.SendAsync(message, cancellationToken)
                .ConfigureAwait(false);

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
            _httpCLient.Dispose();
        }
    }
}
