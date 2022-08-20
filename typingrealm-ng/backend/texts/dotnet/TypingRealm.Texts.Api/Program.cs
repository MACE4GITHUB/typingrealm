using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Texts.Api;
using TypingRealm.Texts.Api.Controllers;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder();

builder.Services.AddControllers();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<Counter>();
builder.Services.AddTransient<ITextRetriever, TextRetriever>();

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());

await app.RunAsync();
