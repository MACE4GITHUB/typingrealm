using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace TypingRealm.Hosting
{
    public static class HostFactory
    {
        public static WebApplicationBuilder CreateWebApiApplicationBuilder(Assembly controllersAssembly)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.UseWebApiHost(builder.Configuration, controllersAssembly);
            builder.Logging.ConfigureTyrLogging();

            return builder;
        }

        public static ILoggingBuilder ConfigureTyrLogging(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss]";
            });

            // TODO:
            // Default level is Information. If we set it to Debug we will log a lot of excessive stuff.
            // We need to leave global level at Information and set "Typingrealm.*" log level to Debug for DEV and Trace for PROD.
            loggingBuilder.SetMinimumLevel(LogLevel.Information);

            return loggingBuilder;
        }
    }
}
