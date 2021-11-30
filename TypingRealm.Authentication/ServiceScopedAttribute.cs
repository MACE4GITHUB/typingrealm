using TypingRealm.Profiles;

namespace TypingRealm.Authentication
{
    public sealed class ServiceScopedAttribute : TyrAuthorizeAttribute
    {
        public ServiceScopedAttribute() : base(TyrScopes.Service, ProfileType.Service)
        {
        }
    }

    public sealed class UserScopedAttribute : TyrAuthorizeAttribute
    {
        public UserScopedAttribute() : base(ProfileType.User)
        {
        }
    }
}
