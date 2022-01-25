using System.Reflection;
using Microsoft.AspNetCore.Builder;
using TypingRealm.Configuration;
using TypingRealm.Logging;

namespace TypingRealm.Hosting;

public static class HostFactory
{
    public static WebApplicationBuilder CreateWebApiApplicationBuilder(Assembly controllersAssembly)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddTyrConfiguration();
        builder.Services.UseWebApiHost(builder.Configuration, controllersAssembly);
        builder.Logging.AddTyrLogging(builder.Configuration);

        return builder;
    }
}
