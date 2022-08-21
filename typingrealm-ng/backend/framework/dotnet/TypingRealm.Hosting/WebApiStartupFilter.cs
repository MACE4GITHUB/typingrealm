using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace TypingRealm.Hosting;

public sealed class WebApiStartupFilter : IStartupFilter
{
    public const string CorsPolicyName = "CorsPolicy";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            ConfigureCommonTyrApp(app);

            next(app);
        };
    }

    public static void ConfigureCommonTyrApp(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseCors(CorsPolicyName);
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
