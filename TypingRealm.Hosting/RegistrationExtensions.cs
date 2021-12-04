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
        public static readonly string[] CorsAllowedOrigins = new[]
        {
#if DEBUG
            "http://127.0.0.1:4200",
            "https://127.0.0.1:4200",
            "http://localhost:4200",
            "https://localhost:4200",
            "http://typingrealm.com:4200",
            "https://typingrealm.com:4200",
            "http://typingrealm.com",
#endif
            "https://typingrealm.com",
            "https://slava.typingrealm.com"
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
