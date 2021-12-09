using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication;
using TypingRealm.Authentication.Service;
using TypingRealm.Communication;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;
using TypingRealm.SignalR;
using TypingRealm.Tcp;

namespace TypingRealm.Hosting.Service
{
    public static class RegistrationExtensions
    {
        public static MessageTypeCacheBuilder UseTcpHost(this IServiceCollection services, int port)
        {
            Hosting.RegistrationExtensions.SetupCommonDependencies(services);

            services
                .AddProtobuf()
                .AddTcpServer(port);

            services
                .AddCommunication()
                .RegisterMessaging();
            var builder = services.AddSerializationCore();

            builder
                .AddTyrServiceWithoutAspNetAuthentication();

            services.AddHostedService<TcpServerHostedService>();
            services.AddJson(); // Use this to serialize/deserialize messages in JSON instead of protobuf base64 string.

            return builder;
        }

        public static MessageTypeCacheBuilder UseSignalRHost(this IServiceCollection services)
        {
            Hosting.RegistrationExtensions.SetupCommonDependencies(services);

            services.AddSignalR();

            services.AddCors(options => options.AddPolicy(
                Hosting.RegistrationExtensions.CorsPolicyName,
                builder => builder
                    .WithOrigins(Hosting.RegistrationExtensions.CorsAllowedOrigins)
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
                .AddTyrServiceAuthentication();

            services.AddTransient<IStartupFilter, SignalRStartupFilter>();

            services.AddHealthChecks();

            services.AddJson();
            services.AddProtobufMessageSerializer(); // Use this to enable protobuf base64 string message serializer instead of json.

            return builder;
        }
    }
}
