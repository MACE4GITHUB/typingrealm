using System;

namespace TypingRealm.Profiles
{
    public sealed class Profile
    {
        public Profile(string userId)
        {
            ProfileId = ProfileId.ForUser(userId);
            Type = ProfileType.User;
        }

        private Profile(ProfileType type)
        {
            if (type == ProfileType.User)
                throw new InvalidOperationException("Cannot create User profile without ProfileId.");

            Type = type;

            if (type == ProfileType.Anonymous)
                ProfileId = ProfileId.Anonymous();

            if (type == ProfileType.Service)
                ProfileId = ProfileId.ForService();

            if (ProfileId == null)
                throw new InvalidOperationException("Could not correctly initialize ProfileId field.");
        }

        public ProfileId ProfileId { get; }
        public ProfileType Type { get; }

        /// <summary>
        /// Session is authenticated whether as a Service or as a User.
        /// </summary>
        public bool IsAuthenticated => Type != ProfileType.Anonymous;

        public static Profile Anonymous() => new Profile(ProfileType.Anonymous);
        public static Profile ForService() => new Profile(ProfileType.Service);
    }
}
