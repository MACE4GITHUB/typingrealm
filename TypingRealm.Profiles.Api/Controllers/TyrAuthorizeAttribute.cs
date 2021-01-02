using Microsoft.AspNetCore.Mvc;

namespace TypingRealm.Profiles.Api.Controllers
{
    public class TyrAuthorizeAttribute : TypeFilterAttribute
    {
        public TyrAuthorizeAttribute(string scope) : base(typeof(ScopeAuthorizationFilter))
        {
            Arguments = new object[] { scope };
        }
    }
}
