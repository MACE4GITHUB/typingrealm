using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypingRealm.Hosting;
using TypingRealm.Texts.Api;

[assembly: ApiController]

var builder = HostFactory.CreateWebApplicationBuilder();

builder.Services.AddSingleton<ITextRetriever>(
    c => new AheadOfTimeTextRetriever(
        c.GetRequiredService<ILogger<AheadOfTimeTextRetriever>>(),
        new TextRetriever(c.GetRequiredService<IHttpClientFactory>())));

var app = builder.Build();

// Start retrieving texts.
app.Services.GetRequiredService<ITextRetriever>();
await app.RunAsync();
