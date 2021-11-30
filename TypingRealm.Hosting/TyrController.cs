using Microsoft.AspNetCore.Mvc;
using TypingRealm.Profiles;

namespace TypingRealm.Hosting
{
    public abstract class TyrController : Controller
    {
        protected Profile Profile
        {
            get
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated)
                    return Profile.Anonymous();

                if (User.Identity.Name == null /* This is not a human. */)
                    return Profile.ForService();

                return new Profile(User.Identity.Name);
            }
        }

        protected ProfileId ProfileId => Profile.ProfileId;
    }
}
