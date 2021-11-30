using System.Security.Claims;

namespace TypingRealm.Profiles
{
    public static class ProfileHelpers
    {
        public static Profile GetProfileForUser(ClaimsPrincipal user)
        {
            if (user.Identity == null || !user.Identity.IsAuthenticated)
                return Profile.Anonymous();

            if (user.Identity.Name == null /* This is not a human. */)
                return Profile.ForService();

            return new Profile(user.Identity.Name);
        }
    }
}
