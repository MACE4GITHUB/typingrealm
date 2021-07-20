using TypingRealm.Authentication;

namespace TypingRealm.Authorization
{
    public sealed class ServiceScopedAttribute : TyrAuthorizeAttribute
    {
        public ServiceScopedAttribute() : base(TyrScopes.Service)
        {
        }
    }
}
