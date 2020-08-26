using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.SignalR;

namespace TypingRealm.RopeWar.Server
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddSerializationCore()
                .AddMessageTypesFromAssembly(typeof(JoinContest).Assembly)
                .AddJson()
                .Services
                .RegisterMessaging()
                .RegisterMessageHub()
                .RegisterRopeWar();
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

                endpoints.MapHub<MessageHub>("/hub");
            });
        }
    }
}
