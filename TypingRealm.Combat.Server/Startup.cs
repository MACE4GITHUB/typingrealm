using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Combat;
using TypingRealm.Combat.Messages;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Handlers;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.SignalR;

namespace TypingRealm.SignalRServer
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddSerializationCore()
                .AddMessageTypesFromAssembly(typeof(Attacked).Assembly)
                .Services
                .AddJson()
                .RegisterMessaging()
                .RegisterMessageHub();

            services.AddSingleton<ICombatRoomStore, CombatRoomStore>();
            services.RegisterHandler<TargetingPlayer, BroadcastMessageHandler>();
            services.RegisterHandler<TargetedPlayer, BroadcastMessageHandler>();
            services.RegisterHandler<Attacking, BroadcastMessageHandler>();
            services.RegisterHandler<Attacked, BroadcastMessageHandler>();
            services.RegisterHandler<Attacked, AttackedHandler>();
            services.AddTransient<IConnectionInitializer, EngageConnectionInitializer>();
            services.UseUpdateFactory<UpdateFactory>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (DebugHelpers.IsDevelopment())
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
