﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using TypingRealm.Hosting;
using TypingRealm.Data.Infrastructure;
using TypingRealm.Data.Api.Controllers;
using TypingRealm.Typing;

[assembly: ApiController]
namespace TypingRealm.Data.Api
{
    public static class Program
    {
        public static async Task Main()
        {
            using var host = HostFactory.CreateWebApiHostBuilder(typeof(LocationsController).Assembly, services =>
            {
                services.RegisterDataApi();

                services.AddTyping();
            }).Build();

            await host.RunAsync();
        }
    }
}
