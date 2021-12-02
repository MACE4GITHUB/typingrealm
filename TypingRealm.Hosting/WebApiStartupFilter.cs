﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;

namespace TypingRealm.Hosting
{
    public sealed class WebApiStartupFilter : IStartupFilter
    {
        private const string CorsPolicyName = "CorsPolicy";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                // Forward HTTP errors throughout services call chain.
                app.UseExceptionHandler(exceptionHandlerApp =>
                {
                    exceptionHandlerApp.Run(context =>
                    {
                        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (contextFeature?.Error is HttpRequestException exception)
                        {
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
                });

                // TODO: Check whether it's Development environment and only then enable swagger.
                app.UseSwagger();
                app.UseSwaggerUI();

                next(app);
            };
        }
    }
}
