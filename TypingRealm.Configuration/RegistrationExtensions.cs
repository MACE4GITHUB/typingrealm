using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace TypingRealm.Configuration
{
    public static class RegistrationExtensions
    {
        public static string GetServiceId(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("ServiceId");
        }

        public static ConfigurationManager AddTyrConfiguration(this ConfigurationManager configuration)
        {
            // A hack to simpler get the service value if not set.
            var entryAssemblyName = Assembly.GetEntryAssembly()?.FullName;
            var parts = entryAssemblyName?.Split('.', ',');
            if (parts != null && parts.Length >= 3 && parts[0] == "TypingRealm" && parts[2] == "Api")
            {
                configuration.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ServiceId", parts[1].ToLowerInvariant())
                });
            }

            var serviceId = configuration.GetServiceId();
            if (string.IsNullOrWhiteSpace(serviceId))
                throw new InvalidOperationException("ServiceId should be specified for service.");

            return configuration;
        }

        // For old configuration way.
        public static IConfigurationBuilder AddTyrConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            // A hack to simpler get the service value if not set.
            var entryAssemblyName = Assembly.GetEntryAssembly()?.FullName;
            var parts = entryAssemblyName?.Split('.', ',');
            if (parts != null && parts.Length >= 3 && parts[0] == "TypingRealm" && parts[2] == "Api")
            {
                configurationBuilder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ServiceId", parts[1].ToLowerInvariant())
                });
            }

            var configuration = configurationBuilder.Build();

            var serviceId = configuration.GetServiceId();
            if (string.IsNullOrWhiteSpace(serviceId))
                throw new InvalidOperationException("ServiceId should be specified for service.");

            return configurationBuilder;
        }
    }
}
