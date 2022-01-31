using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication.Service;
using TypingRealm.Hosting.Service;
using TypingRealm.TypingDuels;

[assembly: ApiController]
var builder = HostFactory.CreateSignalRApplicationBuilder(
    typeof(ControllersAssembly).Assembly, builder =>
    {
        builder.AddTypingDuelsDomain()
            .Services.AddAuthorizeConnectHook();
    });

await builder.Build().RunAsync();

internal sealed class ControllersAssembly { }
