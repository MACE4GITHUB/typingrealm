using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Hosting;
using TypingRealm.Library.Api.Controllers;
using TypingRealm.Library.Infrastructure;
using TypingRealm.Library.Sentences;

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
builder.Services.AddTransient<IValidator<BookIdRouteParameter>, BookIdRouteParameterValidator>();
builder.Services.AddTransient<IValidator<UpdateBookDto>, UpdateBookDtoValidator>();
builder.Services.AddTransient<IValidator<UploadBookDto>, UploadBookDtoValidator>();
builder.Services.AddTransient<IValidator<LanguageQueryParameter>, LanguageQueryParameterValidator>();
builder.Services.AddTransient<IValidator<SentencesRequest>, SentencesRequestValidator>();
builder.Services.AddTransient<IValidator<WordsRequest>, WordsRequestValidator>();

await builder.Build().RunAsync();
