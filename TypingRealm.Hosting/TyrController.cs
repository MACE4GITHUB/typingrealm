using Microsoft.AspNetCore.Mvc;
using TypingRealm.Profiles;

namespace TypingRealm.Hosting
{
    public abstract class TyrController : Controller
    {
        protected Profile Profile => ProfileHelpers.GetProfileForUser(User);

        protected ProfileId ProfileId => Profile.ProfileId;
    }
}
