using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TypingRealm.SignalR;

namespace TypingRealm.Hosting.Service;

public sealed class SignalRStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            WebApiStartupFilter.ConfigureCommonTyrApp(app, endpoints =>
            {
                endpoints.MapHub<MessageHub>("/hub").RequireAuthorization();
            });

            next(app);
        };
    }
}
