using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication;

namespace TypingRealm.Communication;

public sealed class InMemoryServiceClient : IServiceClient
{
    // HACK: Make sure when proper configuration is implemented we are not getting environment variables here.
    // And also in IdentityServerAuthenticationConfiguration.

    private readonly Dictionary<string, string> _serviceAddresses
        = new Dictionary<string, string>
        {
            ["data"] = Environment.GetEnvironmentVariable("DATA_URL") ?? "http://127.0.0.1:30400",
            ["library"] = Environment.GetEnvironmentVariable("LIBRARY_URL") ?? "http://127.0.0.1:30402",
            ["profiles"] = Environment.GetEnvironmentVariable("PROFILES_URL") ?? "http://127.0.0.1:30103",
            ["texts"] = Environment.GetEnvironmentVariable("TEXTS_URL") ?? "http://127.0.0.1:30401",
            ["typing"] = Environment.GetEnvironmentVariable("TYPING_URL") ?? "http://127.0.0.1:30403",
            ["typingduels"] = Environment.GetEnvironmentVariable("TYPINGDUELS_URL") ?? "http://127.0.0.1:30404"
        };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IProfileTokenService _profileTokenService;
    private readonly IServiceTokenService _serviceTokenService;

    public InMemoryServiceClient(
        IHttpClientFactory httpClientFactory,
        IProfileTokenService profileTokenService,
        IServiceTokenService serviceTokenService)
    {
        _httpClientFactory = httpClientFactory;
        _profileTokenService = profileTokenService;
        _serviceTokenService = serviceTokenService;
    }

    public async ValueTask<T> GetAsync<T>(string serviceName, string endpoint, EndpointAuthentication endpointAuthentication, CancellationToken cancellationToken)
    {
        if (!_serviceAddresses.ContainsKey(serviceName))
            throw new InvalidOperationException("Service is not registered in service discovery.");

        var uri = $"{_serviceAddresses[serviceName]}/{endpoint}";

        var endpointAuthenticationType = endpointAuthentication.AuthenticationType;
        var accessToken = endpointAuthenticationType switch
        {
            EndpointAuthenticationType.Profile => await _profileTokenService.GetProfileAccessTokenAsync(cancellationToken).ConfigureAwait(false),
            EndpointAuthenticationType.Service => endpointAuthentication.Credentials == null
                ? await _serviceTokenService.GetServiceAccessTokenAsync(cancellationToken).ConfigureAwait(false)
                : await _serviceTokenService.GetServiceAccessTokenAsync(endpointAuthentication.Credentials, cancellationToken).ConfigureAwait(false),
            _ => null
        };

        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, uri);
            using var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.SendMessageAsync<T>(message, accessToken, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            // TODO: Consider placing it in some other place of code, like IHttpClient.
            // TODO: Fix the issue that if endpoint return STRUCT - we return default value when it's not found!
            // TODO: What if URL is incorrect - it shouldn't return default value.
            // TODO: If object is valueobject - we cannot return default value. We always need to return NULL.
            // Consider wrapping response in some kind of Result object.
            if (exception.StatusCode == HttpStatusCode.NotFound)
                return default!;

            throw;
        }
    }

    public async ValueTask PostAsync<T>(string serviceName, string endpoint, EndpointAuthentication endpointAuthentication, T content, CancellationToken cancellationToken)
    {
        if (!_serviceAddresses.ContainsKey(serviceName))
            throw new InvalidOperationException("Service is not registered in service discovery.");

        var uri = $"{_serviceAddresses[serviceName]}/{endpoint}";

        var endpointAuthenticationType = endpointAuthentication.AuthenticationType;
        var accessToken = endpointAuthenticationType switch
        {
            EndpointAuthenticationType.Profile => await _profileTokenService.GetProfileAccessTokenAsync(cancellationToken).ConfigureAwait(false),
            EndpointAuthenticationType.Service => endpointAuthentication.Credentials == null
                ? await _serviceTokenService.GetServiceAccessTokenAsync(cancellationToken).ConfigureAwait(false)
                : await _serviceTokenService.GetServiceAccessTokenAsync(endpointAuthentication.Credentials, cancellationToken).ConfigureAwait(false),
            _ => null
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, uri);
        using var httpClient = _httpClientFactory.CreateClient();
        await httpClient.SendMessageAsync(message, accessToken, content, cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<TResponse> PostAsync<TBody, TResponse>(string serviceName, string endpoint, EndpointAuthentication endpointAuthentication, TBody content, CancellationToken cancellationToken)
    {
        if (!_serviceAddresses.ContainsKey(serviceName))
            throw new InvalidOperationException("Service is not registered in service discovery.");

        var uri = $"{_serviceAddresses[serviceName]}/{endpoint}";

        var endpointAuthenticationType = endpointAuthentication.AuthenticationType;
        var accessToken = endpointAuthenticationType switch
        {
            EndpointAuthenticationType.Profile => await _profileTokenService.GetProfileAccessTokenAsync(cancellationToken).ConfigureAwait(false),
            EndpointAuthenticationType.Service => endpointAuthentication.Credentials == null
                ? await _serviceTokenService.GetServiceAccessTokenAsync(cancellationToken).ConfigureAwait(false)
                : await _serviceTokenService.GetServiceAccessTokenAsync(endpointAuthentication.Credentials, cancellationToken).ConfigureAwait(false),
            _ => null
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, uri);
        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.SendMessageAsync<TResponse, TBody>(message, accessToken, content, cancellationToken)
            .ConfigureAwait(false);

        return response;
    }

    public async ValueTask DeleteAsync(string serviceName, string endpoint, EndpointAuthentication endpointAuthentication, CancellationToken cancellationToken)
    {
        if (!_serviceAddresses.ContainsKey(serviceName))
            throw new InvalidOperationException("Service is not registered in service discovery.");

        var uri = $"{_serviceAddresses[serviceName]}/{endpoint}";

        var endpointAuthenticationType = endpointAuthentication.AuthenticationType;
        var accessToken = endpointAuthenticationType switch
        {
            EndpointAuthenticationType.Profile => await _profileTokenService.GetProfileAccessTokenAsync(cancellationToken).ConfigureAwait(false),
            EndpointAuthenticationType.Service => endpointAuthentication.Credentials == null
                ? await _serviceTokenService.GetServiceAccessTokenAsync(cancellationToken).ConfigureAwait(false)
                : await _serviceTokenService.GetServiceAccessTokenAsync(endpointAuthentication.Credentials, cancellationToken).ConfigureAwait(false),
            _ => null
        };

        using var message = new HttpRequestMessage(HttpMethod.Delete, uri);
        using var httpClient = _httpClientFactory.CreateClient();
        await httpClient.SendMessageAsync(message, accessToken, cancellationToken)
            .ConfigureAwait(false);
    }
}
