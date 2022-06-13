using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TypingRealm.Profiles;

namespace TypingRealm.Authentication.Api;

// TODO: Combine token service & profile service to be able to just get the token & profile information.
public interface IProfileService
{
    ValueTask<AuthenticatedProfile?> TryGetProfileAsync();
}

public sealed class HttpContextProfileService : IProfileService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextProfileService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ValueTask<AuthenticatedProfile?> TryGetProfileAsync()
    {
        if (_httpContextAccessor.HttpContext == null)
            throw new NotSupportedException("HttpContext is not available, cannot acquire profile.");

        if (_httpContextAccessor.HttpContext?.User?.Identity == null
            || !_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            return new((AuthenticatedProfile?)null); // No profile. Consider throwing an exception here.

        var profile = AuthenticatedProfile.GetProfileForUser(_httpContextAccessor.HttpContext.User);

        return new(profile);
    }
}
