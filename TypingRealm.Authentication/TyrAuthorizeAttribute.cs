using Microsoft.AspNetCore.Mvc;

namespace TypingRealm.Authorization
{
    public class TyrAuthorizeAttribute : TypeFilterAttribute
    {
        public TyrAuthorizeAttribute(string scope) : base(typeof(ScopeAuthorizationFilter))
        {
            Arguments = new object[] { scope };
        }
    }
}
