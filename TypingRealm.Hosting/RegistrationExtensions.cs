using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication;
using TypingRealm.Communication;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;
using TypingRealm.SignalR;
using TypingRealm.Tcp;

namespace TypingRealm.Hosting
{
    public static class RegistrationExtensions
    {
        private const string CorsPolicyName = "CorsPolicy";
        private static readonly string[] _corsAllowedOrigins = new[]
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

        public static MessageTypeCacheBuilder UseTcpHost(this IServiceCollection services, int port)
        {
            services.SetupCommonDependencies();

            services
                .AddProtobuf()
                .AddTcpServer(port);

            services
                .AddCommunication()
                .RegisterMessaging();
            var builder = services.AddSerializationCore();

            builder
                .AddTyrServiceWithoutAspNetAuthentication();
                //.UseLocalProvider();

            services.AddHostedService<TcpServerHostedService>();
            services.AddJson(); // Use this to serialize/deserialize messages in JSON instead of protobuf base64 string.

            return builder;
        }

        public static MessageTypeCacheBuilder UseSignalRHost(this IServiceCollection services)
        {
            services.SetupCommonDependencies();

            services.AddSignalR();

            services.AddCors(options => options.AddPolicy(
                CorsPolicyName,
                builder => builder
                    .WithOrigins(_corsAllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

            services
                .RegisterMessageHub();

            services
                .AddCommunication()
                .RegisterMessaging();
            var builder = services.AddSerializationCore();
            builder
                .AddTyrWebServiceAuthentication();
                //.UseLocalProvider();

            services.AddTransient<IStartupFilter, SignalRStartupFilter>();

            services.AddJson();
            services.AddProtobufMessageSerializer(); // Use this to enable protobuf base64 string message serializer instead of json.

            return builder;
        }

        public static IServiceCollection UseWebApiHost(this IServiceCollection services, Assembly controllersAssembly)
        {
            services.SetupCommonDependencies();

            services.AddCors(options => options.AddPolicy(
                CorsPolicyName,
                builder => builder
                    .WithOrigins(_corsAllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

            services.AddCommunication();
            services.AddTyrApiAuthentication();
                //.UseLocalProvider();

            services.AddControllers()
                .PartManager.ApplicationParts.Add(new AssemblyPart(controllersAssembly));

            services.AddTransient<IStartupFilter, WebApiStartupFilter>();

            services.AddSwaggerGen();

            return services;
        }

        private static IServiceCollection SetupCommonDependencies(this IServiceCollection services)
        {
            services.AddHttpClient();

            return services;
        }
    }
}
