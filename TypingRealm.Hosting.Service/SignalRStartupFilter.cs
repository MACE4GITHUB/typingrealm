using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TypingRealm.SignalR;

namespace TypingRealm.Hosting.Service
{
    public sealed class SignalRStartupFilter : IStartupFilter
    {
        private const string CorsPolicyName = "CorsPolicy";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseRouting();
                app.UseCors(CorsPolicyName);
                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<MessageHub>("/hub").RequireAuthorization();
                    endpoints.MapHealthChecks("health");
                });

                next(app);
            };
        }
    }
}
