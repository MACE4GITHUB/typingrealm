using Microsoft.AspNetCore.Mvc;
using TypingRealm.Profiles;

namespace TypingRealm.Authentication
{
    public abstract class TyrAuthorizeAttribute : TypeFilterAttribute
    {
        protected TyrAuthorizeAttribute(string scope) : base(typeof(ScopeAuthorizationFilter))
        {
            Arguments = new object[] { scope };
        }

        protected TyrAuthorizeAttribute(ProfileType profileType) : base(typeof(ProfileTypeAuthorizationFilter))
        {
            Arguments = new object[] { profileType };
        }

        protected TyrAuthorizeAttribute(string scope, ProfileType profileType) : base(typeof(ScopeAndProfileTypeAuthorizationFilter))
        {
            Arguments = new object[] { scope, profileType };
        }
    }
}
