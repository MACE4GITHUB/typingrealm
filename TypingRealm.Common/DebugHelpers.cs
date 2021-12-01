namespace TypingRealm
{
    public static class DebugHelpers
    {
        /// <summary>
        /// Change this to false for production (Auth0) scenario.
        /// </summary>
        public static bool UseLocalAuthentication =>
#if DEBUG
            true;
#else
            false;
#endif

        public static bool UseInfrastructure =>
#if DEBUG
            false;
#else
            true;
#endif
    }
}
