using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TypingRealm.Authentication.Api
{
    public sealed class HttpContextProfileTokenService : IProfileTokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextProfileTokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext == null)
                throw new NotSupportedException("HttpContext is not available, cannot acquire profile access token.");

            if (_httpContextAccessor.HttpContext?.User?.Identity == null
                || !_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("Access token is not set on HTTP context. User is not authenticated.");
            }

            // This might be an interesting feature.
            // _httpContextAccessor.HttpContext.GetAccessTokenAsync() returns the token
            // ONLY when it's the default scheme token, which is Profile scheme.
            // So in theory, previous implementation was good for our use case.

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault();

            if (token == null || !token.StartsWith("Bearer ", StringComparison.Ordinal))
                throw new InvalidOperationException("Access token is not set on HTTP context or has invalid format.");

            token = token.Remove(0, 7);

            return new ValueTask<string>(token);
        }
    }
}
