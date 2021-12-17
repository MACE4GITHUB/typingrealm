namespace TypingRealm.Profiles
{
    public enum ProfileType
    {
        None,

        /// <summary>
        /// If user is not authenticated (no token, invalid token).
        /// </summary>
        Anonymous = 1,

        /// <summary>
        /// If request is authenticated, but there is no sub claim (CC token).
        /// </summary>
        Service = 2,

        /// <summary>
        /// If user is authenticated with valid user sub claim.
        /// </summary>
        User = 3
    }
}
