using System;

namespace TypingRealm
{
    public static class DebugHelpers
    {
        /// <summary>
        /// Change this to false for production (Auth0) scenario.
        /// </summary>
        public static bool UseDevelopmentAuthentication => IsDevelopment();

        public static bool UseInfrastructure => !Convert.ToBoolean(
            Environment.GetEnvironmentVariable("DISABLE_INFRASTRUCTURE"));

        public static bool IsDevelopment()
        {
            return GetEnvironment() == "Development";
        }

        public static string GetEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return environment ?? "Production";
        }
    }
}
