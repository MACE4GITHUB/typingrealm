using Microsoft.Extensions.Logging;
using Serilog;

namespace TypingRealm.Logging
{
    public static class RegistrationExtensions
    {
        public static ILoggingBuilder AddTyrLogging(this ILoggingBuilder builder)
        {
            builder.ClearProviders();

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            builder.AddSerilog(logger);

            return builder;
        }
    }
}
