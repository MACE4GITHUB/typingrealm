using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Library.Api.Controllers;
using TypingRealm.Library.Infrastructure;

[assembly: ApiController]
var builder = HostFactory.CreateWebApiApplicationBuilder(typeof(SentencesController).Assembly);

builder.Services.AddLibraryApi(builder.Configuration);

await builder.Build().RunAsync();
