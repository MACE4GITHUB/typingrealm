using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using TypingRealm.Authentication.Api;

namespace TypingRealm.Hosting;

public sealed class WebApiStartupFilter : IStartupFilter
{
    private const string CorsPolicyName = "CorsPolicy";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            ConfigureCommonTyrApp(app);

            next(app);
        };
    }

    public static void ConfigureCommonTyrApp(IApplicationBuilder app, Action<IEndpointRouteBuilder>? configureEndpoints = null)
    {
        // Forward HTTP errors throughout services call chain.
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(context =>
            {
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature?.Error is HttpRequestException exception)
                {
                        // TODO: Consider forwarding only 403, 400 or 404 forwarding might be undesired.
                        context.Response.StatusCode = (int)(exception.StatusCode ?? HttpStatusCode.InternalServerError);
                }

                return Task.CompletedTask;
            });
        });

        app.UseRouting();
        app.UseCors(CorsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireAuthorization();
            endpoints.MapHealthChecks("ping", new HealthCheckOptions
            {
                Predicate = _ => false
            });
            endpoints.MapHealthChecks("health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapHealthChecks("health/services", new HealthCheckOptions
            {
                Predicate = reg => reg.Tags.Contains("service"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapHealthChecks("health/infrastructure", new HealthCheckOptions
            {
                Predicate = reg => reg.Tags.Contains("infrastructure"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            configureEndpoints?.Invoke(endpoints);
        });

        // TODO: Check whether it's Development environment and only then enable swagger.
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
