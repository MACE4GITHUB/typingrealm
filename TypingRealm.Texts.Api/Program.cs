using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Texts;
using TypingRealm.Texts.Api.Controllers;

[assembly: ApiController]
var builder = HostFactory.CreateWebApiApplicationBuilder(typeof(TextsController).Assembly);

builder.Services.AddTextsApi();

await builder
    .Build()
    .RunAsync();
