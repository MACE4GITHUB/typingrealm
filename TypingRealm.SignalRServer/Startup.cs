using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.SignalRServer
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddSerializationCore().Services
                .RegisterMessaging();

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
