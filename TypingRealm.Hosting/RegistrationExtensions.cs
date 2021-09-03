using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication;
using TypingRealm.Authentication.ConsoleClient;
using TypingRealm.Communication;
using TypingRealm.Data.Api.Client;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.RopeWar;
using TypingRealm.SignalR;
using TypingRealm.SignalR.Client;
using TypingRealm.Tcp;
using TypingRealm.World;

namespace TypingRealm.Hosting
{
    public static class RegistrationExtensions
    {
        private const string CorsPolicyName = "CorsPolicy";
        private static readonly string[] _corsAllowedOrigins = new[]
        {
            "http://localhost:4200",
            "https://localhost:4200",
            "http://typingrealm.com:4200",
            "https://typingrealm.com:4200"
        };

        public static MessageTypeCacheBuilder UseTcpHost(this IServiceCollection services, int port)
        {
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

            return services;
        }

        /// <summary>
        /// This is a specific host that shouldn't register any server-side logic, only main framework for the front-end.
        /// </summary>
        public static MessageTypeCacheBuilder UseConsoleAppHost(this IServiceCollection services)
        {
            var builder = services.AddSerializationCore();

            builder
                .AddTyrAuthenticationMessages()
                .AddWorldMessages()
                .AddRopeWarMessages();

            services
                .AddCommunication()
                .AddJson()
                .AddProtobufMessageSerializer()
                .RegisterClientMessaging() // Client-specific. TODO: use RegisterClientMessagingBase instead.
                .AddSignalRConnectionFactory()
                .AddProfileApiClients()
                .AddLocationApiClients()
                .RegisterClientConnectionFactoryFactory<SignalRClientConnectionFactoryFactory>()
                .AddAuth0ProfileTokenProvider();

            return builder;
        }
    }
}
