using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Messaging.Serialization.Json
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddJson(this IServiceCollection services)
        {
            services.AddTransient<IMessageSerializer>(_ =>
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                options.Converters.Add(new JsonStringEnumConverter());

                return new JsonMessageSerializer(options);
            });

            return services;
        }
    }
}
