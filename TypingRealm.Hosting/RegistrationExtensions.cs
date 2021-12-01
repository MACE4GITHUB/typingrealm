using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.Api;
using TypingRealm.Communication;

namespace TypingRealm.Hosting
{
    public static class RegistrationExtensions
    {
        public static readonly string CorsPolicyName = "CorsPolicy";
        public static readonly string[] CorsAllowedOrigins = new[]
        {
            "http://127.0.0.1:4200",
            "https://127.0.0.1:4200",
            "http://localhost:4200",
            "https://localhost:4200",
            "http://typingrealm.com:4200",
            "https://typingrealm.com:4200",
            "http://typingrealm.com",
            "https://typingrealm.com"
        };

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

            services.AddControllers()
                .PartManager.ApplicationParts.Add(new AssemblyPart(controllersAssembly));

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
