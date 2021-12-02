using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Communication
{
    public static class HttpClientExtensions
    {
        public static async ValueTask<TResponse> SendMessageAsync<TResponse>(
            this HttpClient httpClient, HttpRequestMessage message, string? accessToken, CancellationToken cancellationToken)
        {
            // TODO: Use serialization from Messaging.Serialization.Core, don't duplicate code.
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new JsonStringEnumConverter());

            if (accessToken != null)
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(message, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (typeof(TResponse).IsPrimitive || typeof(TResponse) == typeof(string))
                return (TResponse)Convert.ChangeType(content, typeof(TResponse));

            var deserialized = JsonSerializer.Deserialize<TResponse>(content, options);
            if (deserialized == null)
                throw new InvalidOperationException("Could not deserialize response from API.");

            return deserialized;
        }

        public static async ValueTask<TResponse> SendMessageAsync<TResponse, TBody>(
            this HttpClient httpClient, HttpRequestMessage message, string? accessToken, TBody body, CancellationToken cancellationToken)
        {
            // TODO: Use serialization from Messaging.Serialization.Core, don't duplicate code.
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new JsonStringEnumConverter());

            using var bodyStringContent = new StringContent(
                JsonSerializer.Serialize(body, options: options),
                Encoding.UTF8,
                "application/json");
            message.Content = bodyStringContent;

            if (accessToken != null)
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(message, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (typeof(TResponse).IsPrimitive || typeof(TResponse) == typeof(string))
                return (TResponse)Convert.ChangeType(content, typeof(TResponse));

            var deserialized = JsonSerializer.Deserialize<TResponse>(content, options);
            if (deserialized == null)
                throw new InvalidOperationException("Could not deserialize response from API.");

            return deserialized;
        }

        public static async ValueTask SendMessageAsync<TBody>(
            this HttpClient httpClient, HttpRequestMessage message, string? accessToken, TBody body, CancellationToken cancellationToken)
        {
            // TODO: Use serialization from Messaging.Serialization.Core, don't duplicate code.
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new JsonStringEnumConverter());

            using var bodyStringContent = new StringContent(
                JsonSerializer.Serialize(body, options: options),
                Encoding.UTF8,
                "application/json");
            message.Content = bodyStringContent;

            if (accessToken != null)
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(message, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }

        public static async ValueTask SendMessageAsync(
            this HttpClient httpClient, HttpRequestMessage message, string? accessToken, CancellationToken cancellationToken)
        {
            if (accessToken != null)
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(message, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }
    }
}
