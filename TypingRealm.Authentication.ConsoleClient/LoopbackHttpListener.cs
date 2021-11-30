using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TypingRealm.Authentication.ConsoleClient
{
    public class LoopbackHttpListener : IDisposable
    {
        private const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)

        private readonly IWebHost _host;
        private readonly TaskCompletionSource<string> _source = new TaskCompletionSource<string>();

        public string Url { get; }

        public LoopbackHttpListener(int port, string? path = null)
        {
            path ??= string.Empty;
            if (path.StartsWith("/")) path = path[1..];

            Url = $"http://127.0.0.1:{port}/{path}";

            _host = new WebHostBuilder()
                .UseUrls(Url)
                .UseKestrel()
                .Configure(Configure)
                .Build();
            _host.Start();
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await Task.Delay(500).ConfigureAwait(false);
                _host.Dispose();
            });
        }

        private void Configure(IApplicationBuilder app)
        {
            app.Run(async ctx =>
            {
                if (ctx.Request.Method == "GET")
                {
                    if (ctx.Request.QueryString.Value == null)
                        throw new InvalidOperationException("QueryString value is null.");

                    SetResult(ctx.Request.QueryString.Value, ctx);
                }
                else if (ctx.Request.Method == "POST")
                {
                    if (ctx.Request.ContentType != null && !ctx.Request.ContentType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Response.StatusCode = 415;
                    }
                    else
                    {
                        using var sr = new StreamReader(ctx.Request.Body, Encoding.UTF8);

                        var body = await sr.ReadToEndAsync().ConfigureAwait(false);
                        await SetResult(body, ctx).ConfigureAwait(false);
                    }
                }
                else
                {
                    ctx.Response.StatusCode = 405;
                }
            });
        }

        private async ValueTask SetResult(string value, HttpContext ctx)
        {
            try
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                await ctx.Response.WriteAsync("<h1>You can now return to the application.</h1>").ConfigureAwait(false);
                await ctx.Response.Body.FlushAsync().ConfigureAwait(false);

                _source.TrySetResult(value);
            }
            catch
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                await ctx .Response.WriteAsync("<h1>Invalid request.</h1>").ConfigureAwait(false);
                await ctx.Response.Body.FlushAsync().ConfigureAwait(false);
            }
        }

        public Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeoutInSeconds * 1000).ConfigureAwait(false);
                _source.TrySetCanceled();
            });

            return _source.Task;
        }
    }
}
