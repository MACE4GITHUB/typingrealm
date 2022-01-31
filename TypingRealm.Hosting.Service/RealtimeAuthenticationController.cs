using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication;
using TypingRealm.Authentication.Api;
using TypingRealm.Communication;

namespace TypingRealm.Hosting.Service;

public sealed record TokenData(string token);

[Route("api/realtime-auth")]
public sealed class RealtimeAuthenticationController : TyrController
{
    private readonly ITyrCache _cache;
    private readonly IServiceTokenService _serviceTokenService;

    public RealtimeAuthenticationController(ITyrCache cache, IServiceTokenService serviceTokenService)
    {
        _cache = cache;
        _serviceTokenService = serviceTokenService;
    }

    [UserScoped]
    [HttpPost]
    [Route("generate")]
    public async ValueTask<ActionResult<string>> GenerateAuthenticationToken()
    {
        var accessToken = await _serviceTokenService.GetServiceAccessTokenAsync(
            new ClientCredentials("realtime-auth", "secret", new[] { "realtime-auth" }), default)
            .ConfigureAwait(false);

        var value = $"{ProfileId.Value}${accessToken}";
        if (value.Count(x => x == '$') > 1)
            throw new InvalidOperationException("Invalid ProfileId: should not contain $ signs.");

        var token = $"{Guid.NewGuid()}_{Convert.ToBase64String(RandomNumberGenerator.GetBytes(20))}";

        await _cache.SetValueAsync(token, value, TimeSpan.FromSeconds(20))
            .ConfigureAwait(false);

        return token;
    }

    [ServiceScoped] // TODO: Consider scoping to some custom scope.
    [Scoped("realtime-auth")]
    [HttpPost]
    [Route("validate")]
    public async ValueTask<string?> ValidateToken([FromBody] TokenData tokenData)
    {
        var token = tokenData.token;

        // We cannot pop it as multiple requests are going on for the same token when negotiating SignalR connection.
        var value = await _cache.GetValueAsync(token)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(value))
            return null;

        // We cannot do this because this is scoped to Service tokens as of now.
        /*if (ProfileId.Value != value.Trim('"').Split('$')[0])
            return null;*/

        // TODO: Validate token.

        return value.Trim('"').Split('$')[1];
    }
}
