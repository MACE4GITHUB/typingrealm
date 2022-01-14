using System.Reflection;
using Microsoft.AspNetCore.Builder;
using TypingRealm.Logging;

namespace TypingRealm.Hosting
{
    public static class HostFactory
    {
        public static WebApplicationBuilder CreateWebApiApplicationBuilder(Assembly controllersAssembly)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.UseWebApiHost(builder.Configuration, controllersAssembly);
            builder.Logging.AddTyrLogging();

            return builder;
        }
    }
}
