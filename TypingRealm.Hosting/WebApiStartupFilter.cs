using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;

namespace TypingRealm.Hosting
{
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
                endpoints.MapHealthChecks("health");

                configureEndpoints?.Invoke(endpoints);
            });

            // TODO: Check whether it's Development environment and only then enable swagger.
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
