﻿using System;
using Microsoft.AspNetCore.Builder;
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
