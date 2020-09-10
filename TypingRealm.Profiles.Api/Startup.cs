﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Authentication;
using TypingRealm.Profiles.Infrastructure;

[assembly: ApiController]
namespace TypingRealm.Profiles.Api
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
            services.AddCors(options => options.AddPolicy(
                CorsPolicyName,
                builder => builder
                    .WithOrigins(_corsAllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

            services.AddTypingRealmAuthentication();
            services.AddControllers();

            services.RegisterProfilesApi();
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
                    await context.Response.WriteAsync("=== TypingRealm Profiles API ===").ConfigureAwait(false);
                });

                endpoints.MapControllers().RequireAuthorization();
            });
        }
    }
}