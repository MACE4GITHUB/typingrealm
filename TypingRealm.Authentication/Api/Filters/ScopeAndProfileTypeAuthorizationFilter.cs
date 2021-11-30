using Microsoft.AspNetCore.Mvc.Filters;
using TypingRealm.Profiles;

namespace TypingRealm.Authentication.Api.Filters
{
    public sealed class ScopeAndProfileTypeAuthorizationFilter : IAuthorizationFilter
    {
        private readonly ScopeAuthorizationFilter _scopeAuthorizationFilter;
        private readonly ProfileTypeAuthorizationFilter _profileTypeAuthorizationFilter;

        public ScopeAndProfileTypeAuthorizationFilter(string scope, ProfileType profileType)
        {
            _scopeAuthorizationFilter = new ScopeAuthorizationFilter(scope);
            _profileTypeAuthorizationFilter = new ProfileTypeAuthorizationFilter(profileType);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            _scopeAuthorizationFilter.OnAuthorization(context);
            _profileTypeAuthorizationFilter.OnAuthorization(context);
        }
    }
}
