using TypingRealm.Authentication;

namespace TypingRealm.Profiles.Api.Controllers
{
    public sealed class ServiceScopedAttribute : TyrAuthorizeAttribute
    {
        public ServiceScopedAttribute() : base(TyrScopes.Service)
        {
        }
    }
}
