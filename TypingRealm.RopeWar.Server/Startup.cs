using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Authentication;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.SignalR;

namespace TypingRealm.RopeWar.Server
{
    public sealed class Startup
    {
        private const string CorsPolicyName = "CorsPolicy";
        private static readonly string[] _corsAllowedOrigins = new[]
        {
            "http://localhost:4200",
            "https://localhost:4200",
            "http://typingrealm.com:4200",
            "https://typingrealm.com:4200"
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddCors(options => options.AddPolicy(
                CorsPolicyName,
                builder => builder
                    .WithOrigins(_corsAllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

            services.AddTyrWebServiceAuthentication()
                .UseLocalProvider();

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
            app.UseCors(CorsPolicyName);
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("=== TypingRealm server ===").ConfigureAwait(false);
                });

                endpoints.MapHub<MessageHub>("/hub").RequireAuthorization();
            });
        }
    }
}
