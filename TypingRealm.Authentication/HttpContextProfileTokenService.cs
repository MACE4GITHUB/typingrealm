using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace TypingRealm.Authentication
{
    public sealed class HttpContextProfileTokenService : IProfileTokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextProfileTokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async ValueTask<string> GetProfileAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext == null)
                throw new NotSupportedException("HttpContext is not available, cannot acquire profile access token.");

            var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token")
                .ConfigureAwait(false);

            if (token == null)
                throw new InvalidOperationException("Access token is not set on HTTP context.");

            return token;
        }
    }
}
