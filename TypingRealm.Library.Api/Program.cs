using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Library;
using TypingRealm.Library.Api.Controllers;

[assembly: ApiController]
var builder = HostFactory.CreateWebApiApplicationBuilder(typeof(LibraryController).Assembly);

builder.Services.AddLibraryDomain();

await builder
    .Build()
    .RunAsync();
