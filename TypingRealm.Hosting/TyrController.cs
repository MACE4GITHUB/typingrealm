using Microsoft.AspNetCore.Mvc;
using TypingRealm.Profiles;

namespace TypingRealm.Hosting;

public abstract class TyrController : Controller
{
    protected AuthenticatedProfile Profile => AuthenticatedProfile.GetProfileForUser(User);

    protected ProfileId ProfileId => Profile.ProfileId;
}
