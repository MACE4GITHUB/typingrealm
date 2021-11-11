using Microsoft.AspNetCore.Mvc;
using TypingRealm.Profiles;

namespace TypingRealm.Data.Api.Controllers
{
    public abstract class TyrController : Controller
    {
        protected ProfileId ProfileId
        {
            get
            {
                // TODO: Move this whole code of getting the Profile API to some common assembly.
                // TODO: Consider creating middleware to get the Profile from ProfileAPI and cache it.
                // Do this as soon as I need more runtime data except just ProfileId: check that profile exists, not disabled, etc.

                if (User.Identity == null || !User.Identity.IsAuthenticated)
                    return new ProfileId(ProfileType.Anonymous);

                if (User.Identity.Name == null /* This is not a human. */)
                    return new ProfileId(ProfileType.Service);

                return new ProfileId(User.Identity.Name);
            }
        }
    }
}
