using TypingRealm.Profiles;

namespace TypingRealm.Authentication.Api;

public sealed class ServiceScopedAttribute : TyrAuthorizeAttribute
{
    public ServiceScopedAttribute() : base(TyrScopes.Service, ProfileType.Service)
    {
    }
}
