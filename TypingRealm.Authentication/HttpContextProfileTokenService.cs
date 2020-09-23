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
            return await _httpContextAccessor.HttpContext.GetTokenAsync("access_token")
                .ConfigureAwait(false);
        }
    }
}
