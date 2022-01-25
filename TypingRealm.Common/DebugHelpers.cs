using System;

namespace TypingRealm;

// TODO: Do not use static class, move out this configuration to a proper
// IConfiguration dependency and unit test it.
public static class DebugHelpers
{
    /// <summary>
    /// Change this to false for production (Auth0) scenario.
    /// </summary>
    public static bool UseDevelopmentAuthentication => IsDevelopment();

    public static bool UseInfrastructure => !Convert.ToBoolean(
        Environment.GetEnvironmentVariable("DISABLE_INFRASTRUCTURE"));

    public static bool IsDeployment()
    {
        var isDeployment = Convert.ToBoolean(Environment.GetEnvironmentVariable("DEPLOYMENT"));
        return isDeployment;
    }

    public static bool IsDevelopment()
    {
        var environment = GetEnvironment();
        return environment == "Development" || environment == "Debug" || environment == "Local";
    }

    public static string GetEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return environment ?? "Production";
    }
}
