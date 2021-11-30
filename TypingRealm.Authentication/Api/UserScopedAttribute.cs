using TypingRealm.Profiles;

namespace TypingRealm.Authentication.Api
{
    public sealed class UserScopedAttribute : TyrAuthorizeAttribute
    {
        public UserScopedAttribute() : base(ProfileType.User)
        {
        }
    }
}
