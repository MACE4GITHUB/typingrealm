using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TypingRealm.Hosting;
using TypingRealm.Typing.Api.Controllers;
using TypingRealm.Typing.Infrastructure;

[assembly: ApiController]
var builder = HostFactory.CreateWebApiApplicationBuilder(typeof(TypingSessionsController).Assembly);

var cacheConnectionString = builder.Configuration.GetConnectionString("CacheConnection");
var dataCacheConnectionString = builder.Configuration.GetConnectionString("ServiceCacheConnection");
builder.Services.AddTypingApi(builder.Configuration, cacheConnectionString, dataCacheConnectionString);

await builder.Build().RunAsync();
