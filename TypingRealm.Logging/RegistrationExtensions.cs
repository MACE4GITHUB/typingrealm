using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace TypingRealm.Logging
{
    public static class RegistrationExtensions
    {
        public static ILoggingBuilder AddTyrLogging(this ILoggingBuilder builder)
        {
            builder.ClearProviders();
            builder.AddSerilog(CreateSerilogLogger());

            return builder;
        }

        private static Serilog.ILogger CreateSerilogLogger()
        {
            var isDevelopment = DebugHelpers.IsDevelopment();

            return new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Information()
                .MinimumLevel.Override("TypingRealm", isDevelopment ? LogEventLevel.Verbose : LogEventLevel.Debug)
                .CreateLogger();
        }
    }
}
