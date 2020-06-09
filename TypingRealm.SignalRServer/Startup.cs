using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Domain;
using TypingRealm.Domain.Infrastructure;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Handling;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;

namespace TypingRealm.SignalRServer
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddSerializationCore()
                .AddDomainCore()
                .AddJson()
                .Services
                .RegisterMessaging()
                .AddDomain()
                .AddDomainInfrastructure();

            // When we use Startup.ConfigureServices, we need to register
            // service locator as singleton if it uses IServiceProvider as a
            // dependency, or IServiceProvider will get disposed.
            services.AddSingleton<IMessageHandlerFactory, MessageHandlerFactory>();

            services.AddSingleton<ActiveConnectionCache>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("=== TypingRealm server ===").ConfigureAwait(false);
                });

                endpoints.MapHub<JsonSerializedMessageHub>("/hub");
            });
        }
    }
}
