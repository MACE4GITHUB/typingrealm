using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypingRealm.Texts.Api;
using TypingRealm.Texts.Api.Controllers;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder();

builder.Services.AddControllers();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<Counter>();
builder.Services.AddSingleton<ITextRetriever>(
    c => new AheadOfTimeTextRetriever(
        c.GetRequiredService<ILogger<AheadOfTimeTextRetriever>>(),
        new TextRetriever(c.GetRequiredService<IHttpClientFactory>())));

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());

// Start retrieving texts.
app.Services.GetRequiredService<ITextRetriever>();
await app.RunAsync();
