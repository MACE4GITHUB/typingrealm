using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication;
using TypingRealm.Authentication.ConsoleClient;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;
using TypingRealm.Tcp.Client;

namespace TypingRealm.Chat.ConsoleApp
{
    public static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("=== TypingRealm chat ===");

            var services = new ServiceCollection();

            services.AddAuth0ProfileTokenProvider();
            if (DebugHelpers.UseLocalAuthentication)
            {
                Console.Write("Profile for local token: ");
                var profile = Console.ReadLine() ?? "default";

                services.AddLocalProfileTokenProvider(profile);
            }

            var provider = services
                .AddLogging() // TODO: Log only to file, console is needed for UI.
                .AddSerializationCore()
                .AddTyrAuthenticationMessages()
                .AddChatMessages()
                .Services
                .AddProtobuf() // Also adds Protobuf ConnectionFactory.
                .AddJson() // Serialize messages with JSON instead of Protobuf.
                //.UseTcpProtobufClientConnectionFactory("typingrealm.com", 40010)
                .UseTcpProtobufClientConnectionFactory("127.0.0.1", 40010)
                .RegisterClientMessaging()
                .BuildServiceProvider();

            Console.Write("Your name: ");
            var characterName = Console.ReadLine() ?? "default";

            // HACK: Authenticate early on so application freezes only in the beginning (fill the cache).
            var tokenProvider = provider.GetRequiredService<IProfileTokenProvider>();
            var token = await tokenProvider.SignInAsync(default).ConfigureAwait(false);

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using var content = new StringContent(JsonSerializer.Serialize(new
            {
                name = characterName
            }), Encoding.UTF8, "application/json");
            //var response = await httpClient.PostAsync("http://typingrealm.com:30103/api/characters", content).ConfigureAwait(false);
            var response = await httpClient.PostAsync("http://127.0.0.1:30103/api/characters", content).ConfigureAwait(false);

            var character = JsonSerializer.Deserialize<Character>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            var clientId = character?.characterId ?? throw new InvalidOperationException("Could not create a character.");
            var group = "lobby";

            var messageProcessor = provider.GetRequiredService<IMessageProcessor>();

            using var cts = new CancellationTokenSource();
            await messageProcessor.ConnectAsync(clientId, cts.Token).ConfigureAwait(false);

            var @lock = new object();
            messageProcessor.Subscribe<MessageLog>(log =>
            {
                lock (@lock)
                {
                    Console.Clear();

                    foreach (var message in log.Messages)
                    {
                        Console.WriteLine(message);
                    }

                    Console.Write("> ");

                    return default;
                }
            });

            await messageProcessor.SendAsync(new Authenticate(token), default)
                .ConfigureAwait(false);

            await messageProcessor.SendAsync(new Connect(clientId, group), default)
                .ConfigureAwait(false);

            while (true)
            {
                var message = Console.ReadLine();
                if (message != null)
                {
                    await messageProcessor.SendAsync(new Say(message), cts.Token)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}
