using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TypingRealm.Authentication
{
    public sealed class ScopeAuthorizationFilter : IAuthorizationFilter
    {
        private readonly string _scope;

        public ScopeAuthorizationFilter(string scope)
        {
            _scope = scope;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var scopes = context.HttpContext.User.Claims
                .Where(claim => claim.Type == "scope") // IdentityServer has multiple claims with type 'scope'.
                .SelectMany(claim => claim.Value.Split(' ')); // Auth0 has one claim 'scope' with all scopes delimited by space.

            var hasScope = scopes.Any(scope => scope == _scope);
            if (!hasScope)
                context.Result = new ForbidResult();
        }
    }
}
