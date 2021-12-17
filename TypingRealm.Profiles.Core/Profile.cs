using System;
using System.Security.Claims;

namespace TypingRealm.Profiles
{
    /// <summary>
    /// Profile is an Authentication & Access concept shared between all services.
    /// Every access token corresponds to a separate Profile. Profile is basically
    /// a wrapper around User's Identity (from the access token).
    /// Profile can be of 3 types: User, Service and Anonymous.
    /// </summary>
    public sealed class Profile
    {
        /// <summary>
        /// Creates a new Profile from the User's Identity (from the access token).
        /// </summary>
        /// <param name="userId">Authenticated user's Identity.</param>
        private Profile(string userId)
        {
            ProfileId = ProfileId.ForUser(userId);
            Type = ProfileType.User;
        }

        /// <summary>
        /// This constructor is used to create Service & Anonymous profiles.
        /// It doesn't work with User profile type.
        /// </summary>
        /// <param name="type">Profile type: Service or Anonymous.</param>
        /// <exception cref="InvalidOperationException">Thrown when User profile
        /// type is specified.</exception>
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
                throw new NotSupportedException($"Unsupported ProfileType: {type}");
        }

        /// <summary>
        /// Profile Identity: Identity of authenticated User, "Service" or "Anonymous".
        /// </summary>
        public ProfileId ProfileId { get; }

        /// <summary>
        /// Profile type: Should be "User" for successfully authenticated user.
        /// </summary>
        public ProfileType Type { get; }

        /// <summary>
        /// Session is authenticated whether as a Service or as a User.
        /// </summary>
        public bool IsAuthenticated => Type != ProfileType.Anonymous;

        public static Profile Anonymous() => new(ProfileType.Anonymous);
        public static Profile ForService() => new(ProfileType.Service);

        /// <summary>
        /// Gets Profile from ClaimsPrincipal of the authenticated request.
        /// If the User is not authenticated - returns Anonymous profile.
        /// If the request is authenticated with CC token - returns Service profile.
        /// </summary>
        public static Profile GetProfileForUser(ClaimsPrincipal user)
        {
            if (user.Identity == null || !user.Identity.IsAuthenticated)
                return Anonymous();

            if (user.Identity.Name == null /* This is not a human. */)
                return ForService();

            return new(user.Identity.Name);
        }
    }
}
