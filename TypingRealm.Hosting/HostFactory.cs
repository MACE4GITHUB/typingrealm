using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace TypingRealm.Hosting
{
    public static class HostFactory
    {
        public static WebApplicationBuilder CreateWebApiApplicationBuilder(Assembly controllersAssembly)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.UseWebApiHost(controllersAssembly);

            return builder;
        }
    }
}
