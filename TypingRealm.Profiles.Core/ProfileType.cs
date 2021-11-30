namespace TypingRealm.Profiles
{
    public enum ProfileType
    {
        /// <summary>
        /// If user is not authenticated (no token, invalid token).
        /// </summary>
        Anonymous = 1,

        /// <summary>
        /// If user is authenticated, but there is no sub claim.
        /// </summary>
        Service = 2,

        /// <summary>
        /// If user is authenticated with valid user sub claim.
        /// </summary>
        User = 3
    }
}
