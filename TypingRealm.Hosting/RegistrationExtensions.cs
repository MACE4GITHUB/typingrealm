using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.Api;
using TypingRealm.Communication;
using TypingRealm.Communication.Redis;

namespace TypingRealm.Hosting
{
    public static class RegistrationExtensions
    {
        public static readonly string CorsPolicyName = "CorsPolicy";
        public static readonly string[] CorsAllowedOrigins = DebugHelpers.IsDevelopment()
            ? GetDevelopmentAllowedOrigins().ToArray()
            : new[] { "https://typingrealm.com" };

        private static IEnumerable<string> GetDevelopmentAllowedOrigins()
        {
            var hosts = new[]
            {
                "127.0.0.1",
                "localhost",
                "typingrealm.com",
                "dev.typingrealm.com"
            };

            var ports = new[]
            {
                "",
                ":4200"
            };

            foreach (var host in hosts)
            {
                foreach (var port in ports)
                {
                    yield return $"http://{host}{port}";
                    yield return $"https://{host}{port}";
                }
            }
        }

        public static IServiceCollection UseWebApiHost(
            this IServiceCollection services, IConfiguration configuration, Assembly controllersAssembly)
        {
            SetupCommonDependencies(services, configuration);
            SetupCommonAspNetDependencies<WebApiStartupFilter>(services, controllersAssembly);

            // Authentication.
            services.AddTyrApiAuthentication();

            return services;
        }

        /// <summary>
        /// Used by Web API, SignalR and TCP hosts (by everything).
        /// Registered before host-specific dependencies are added.
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection SetupCommonDependencies(
            IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            services.AddCommunication();

            // Technology specific.
            services.TryAddRedisGlobalCaching(configuration);
            services.TryAddRedisServiceCaching(configuration);

            // Deployment of infrastructure from all hosts.
            services.AddHostedService<InfrastructureDeploymentHostedService>();

            return services;
        }

        /// <summary>
        /// Used by Web API & SignalR hosts that use ASP.Net hosting framework.
        /// Is not used by custom tools / console apps / TCP servers.
        /// </summary>
        public static IServiceCollection SetupCommonAspNetDependencies<TStartupFilter>(IServiceCollection services, Assembly? controllersAssembly = null)
            where TStartupFilter : class, IStartupFilter
        {
            services.AddCors(options => options.AddPolicy(
                CorsPolicyName,
                builder => builder
                    .WithOrigins(CorsAllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

            // TODO: Consider how to add healthchecks for custom TCP hosts (ping address?).
            services.AddHealthChecks();

            // Web API controllers.
            var mvcBuilder = services.AddControllers();
            mvcBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(typeof(DiagnosticsController).Assembly));

            mvcBuilder.AddJsonOptions(options =>
            {
                // Accept enum values as strings to Web API endpoints.
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // If host has custom APIs.
            if (controllersAssembly != null)
            {
                mvcBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(controllersAssembly));
            }

            // Swagger.
            services.AddSwaggerGen();

            // Web API or SingnalR or another custom Startup filter.
            services.AddTransient<IStartupFilter, TStartupFilter>();

            return services;
        }
    }
}
