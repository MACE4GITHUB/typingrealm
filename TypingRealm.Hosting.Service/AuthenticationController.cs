using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication.Api;
using TypingRealm.Communication;

namespace TypingRealm.Hosting.Service;

public sealed record TokenData(string token);

[UserScoped]
[Route("authentication")]
public sealed class AuthenticationController : TyrController
{
    private readonly ITyrCache _cache;

    public AuthenticationController(ITyrCache cache)
    {
        _cache = cache;
    }

    [HttpPost]
    [Route("generate")]
    public async ValueTask<ActionResult<string>> GenerateAuthenticationToken()
    {
        var token = $"{Guid.NewGuid()}_{Convert.ToBase64String(RandomNumberGenerator.GetBytes(20))}";

        await _cache.SetValueAsync(token, ProfileId.Value, TimeSpan.FromSeconds(20))
            .ConfigureAwait(false);

        return token;
    }

    [HttpPost]
    [Route("validate")]
    public async ValueTask<ActionResult<bool>> ValidateToken([FromBody] TokenData tokenData)
    {
        var token = tokenData.token;

        var value = await _cache.GetValueAsync(token)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(value))
            return false;

        if (ProfileId.Value != value.Trim('"'))
            return false;

        return true;

        // TODO: Implement Remove and Pop methods, use Pop.
        //_cache.RemoveValue(token);
    }
}
