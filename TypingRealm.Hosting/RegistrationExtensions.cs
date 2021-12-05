using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.Api;
using TypingRealm.Communication;
using TypingRealm.Data.Api;

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
                "typingrealm.com"
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

        public static IServiceCollection UseWebApiHost(this IServiceCollection services, Assembly controllersAssembly)
        {
            SetupCommonDependencies(services);

            services.AddCors(options => options.AddPolicy(
                CorsPolicyName,
                builder => builder
                    .WithOrigins(CorsAllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

            services.AddCommunication();
            services.AddTyrApiAuthentication();

            var mvcBuilder = services.AddControllers();
            mvcBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(controllersAssembly));
            mvcBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(typeof(DiagnosticsController).Assembly));

            services.AddTransient<IStartupFilter, WebApiStartupFilter>();

            services.AddSwaggerGen();

            return services;
        }

        public static IServiceCollection SetupCommonDependencies(IServiceCollection services)
        {
            services.AddHttpClient();

            return services;
        }
    }
}
