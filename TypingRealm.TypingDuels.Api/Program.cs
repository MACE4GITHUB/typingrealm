using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting.Service;

[assembly: ApiController]
var builder = HostFactory.CreateSignalRApplicationBuilder(typeof(ControllersAssembly).Assembly);

await builder.Build().RunAsync();

internal sealed class ControllersAssembly { }
