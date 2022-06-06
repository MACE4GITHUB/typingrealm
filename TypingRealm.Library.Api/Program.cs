using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Library.Api.Controllers;
using TypingRealm.Library.Infrastructure;

// The Library service serves as the reference WebAPI service of the whole
// project, and as such is covered with meaningful comments.
//
// ApiController attribute can be applied once on the level of the assembly
// instead of every controller class.
[assembly: ApiController]

// This creates a host that needs only API endpoints, registering all common
// implementations shared between all project's Web API services.
var builder = HostFactory.CreateWebApiApplicationBuilder(typeof(SentencesController).Assembly);

// This Library.Infrastructure extension method should register everything
// needed by the whole Library service.
builder.Services.AddLibraryApi(builder.Configuration);

await builder.Build().RunAsync();
