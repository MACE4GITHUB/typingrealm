using TypingRealm.Common;

namespace TypingRealm.Profiles
{
    public sealed class ProfileId : Identity
    {
        private ProfileId(string value) : base(value)
        {
        }

        public static ProfileId ForUser(string userId) => new ProfileId(userId);
        public static ProfileId ForService() => new ProfileId("Service");
        public static ProfileId Anonymous() => new ProfileId("Anonymous");
    }
}
