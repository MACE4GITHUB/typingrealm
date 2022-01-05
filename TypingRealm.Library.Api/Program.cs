using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypingRealm.Hosting;
using TypingRealm.Library.Api.Controllers;
using TypingRealm.Library.Infrastructure;

[assembly: ApiController]
var builder = HostFactory.CreateWebApiApplicationBuilder(typeof(SentencesController).Assembly);

builder.Services.AddLibraryApi(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await OnAppStartup(scope.ServiceProvider);
}

await app.RunAsync();

static async Task OnAppStartup(IServiceProvider serviceProvider)
{
    var infrastructureDeploymentService = serviceProvider.GetRequiredService<IInfrastructureDeploymentService>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    await infrastructureDeploymentService.DeployInfrastructureAsync();
    logger.LogInformation("Database is successfully migrated.");
}
