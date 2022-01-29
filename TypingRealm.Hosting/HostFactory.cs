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
        builder.Logging.AddTyrLogging(builder.Configuration);
        builder.Services.UseWebApiHost(builder.Configuration, controllersAssembly);

        return builder;
    }
}
