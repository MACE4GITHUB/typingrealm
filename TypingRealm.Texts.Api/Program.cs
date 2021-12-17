using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Texts.Api.Controllers;

[assembly: ApiController]
await HostFactory.CreateWebApiApplicationBuilder(typeof(TextsController).Assembly)
    .Build()
    .RunAsync();
