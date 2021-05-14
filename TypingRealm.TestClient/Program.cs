using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication;
using TypingRealm.Authentication.ConsoleClient;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;
using TypingRealm.RopeWar;
using TypingRealm.SignalR;
using TypingRealm.SignalR.Client;
using TypingRealm.Tcp.Client;
using TypingRealm.World;

namespace TypingRealm.TestClient
{
    public sealed class Character
    {
        public string? profileId { get; set; }
        public string? characterId { get; set; }
        public string? name { get; set; }
    }

    public static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("=== TypingRealm test client ===");
            Console.Write("Type of connection (rw - RopeWar SignalR, rwt - RopeWar TCP / Protobuf, w - World SignalR): ");

            await (Console.ReadLine() switch
            {
                "rw" => RopeWarSignalR(),
                "rwt" => RopeWarTcp(),
                "w" => WorldSignalR(),
                _ => throw new InvalidOperationException("Invalid type of connection")
            }).ConfigureAwait(false);
        }

        public static async Task RopeWarTcp()
        {
            var services = new ServiceCollection();

            services.AddAuth0ProfileTokenProvider();
            if (DebugHelpers.UseLocalAuthentication)
            {
                Console.Write("Profile for local token: ");
                var profile = Console.ReadLine() ?? "default";

                services.AddLocalProfileTokenProvider(profile);
            }

            services
                .AddHttpClient()
                .AddLogging() // TODO: Log only to file, console is needed for UI.
                .AddSerializationCore()
                .AddTyrAuthenticationMessages()
                .AddRopeWarMessages()
                .Services
                .AddProtobuf() // Also adds Protobuf ConnectionFactory.
                .AddJson() // Serialize messages with JSON instead of Protobuf.
                .UseTcpProtobufClientConnectionFactory("127.0.0.1", 30112)
                .RegisterClientMessaging();

            var provider = services.BuildServiceProvider();

            // HACK: Authenticate early on so application freezes only in the beginning (fill the cache).
            var tokenProvider = provider.GetRequiredService<IProfileTokenProvider>();
            _ = await tokenProvider.SignInAsync(default).ConfigureAwait(false);

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var messageProcessor = provider.GetRequiredService<MessageProcessor>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

            await Handle(messageProcessor, messageTypes, tokenProvider, httpClientFactory).ConfigureAwait(false);
        }

        public static async Task RopeWarSignalR()
        {
            var services = new ServiceCollection();

            services.AddAuth0ProfileTokenProvider();
            if (DebugHelpers.UseLocalAuthentication)
            {
                Console.Write("Profile for local token: ");
                var profile = Console.ReadLine() ?? "default";

                services.AddLocalProfileTokenProvider(profile);
            }

            services
                .AddHttpClient()
                .AddLogging() // TODO: Log only to file, console is needed for UI.
                .AddSerializationCore()
                .AddTyrAuthenticationMessages()
                .AddRopeWarMessages()
                .Services
                .AddJson()
                .AddProtobufMessageSerializer() // Serialize messages with Protobuf instead of JSON.
                .AddSignalRConnectionFactory()
                .UseSignalRClientConnectionFactory("http://127.0.0.1:30102/hub")
                .RegisterClientMessaging();

            var provider = services.BuildServiceProvider();

            // HACK: Authenticate early on so application freezes only in the beginning (fill the cache).
            var tokenProvider = provider.GetRequiredService<IProfileTokenProvider>();
            _ = await tokenProvider.SignInAsync(default).ConfigureAwait(false);

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var messageProcessor = provider.GetRequiredService<MessageProcessor>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

            await Handle(messageProcessor, messageTypes, tokenProvider, httpClientFactory).ConfigureAwait(false);
        }

        public static async Task WorldSignalR()
        {
            var services = new ServiceCollection();

            services.AddAuth0ProfileTokenProvider();
            if (DebugHelpers.UseLocalAuthentication)
            {
                Console.Write("Profile for local token: ");
                var profile = Console.ReadLine() ?? "default";

                services.AddLocalProfileTokenProvider(profile);
            }

            services
                .AddHttpClient()
                .AddLogging() // TODO: Log only to file, console is needed for UI.
                .AddSerializationCore()
                .AddTyrAuthenticationMessages()
                .AddWorldMessages()
                .Services
                .AddJson()
                .AddProtobufMessageSerializer() // Serialize messages with Protobuf instead of JSON.
                .AddSignalRConnectionFactory()
                .UseSignalRClientConnectionFactory("http://127.0.0.1:30111/hub")
                .RegisterClientMessaging();

            var provider = services.BuildServiceProvider();

            // HACK: Authenticate early on so application freezes only in the beginning (fill the cache).
            var tokenProvider = provider.GetRequiredService<IProfileTokenProvider>();
            _ = await tokenProvider.SignInAsync(default).ConfigureAwait(false);

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var messageProcessor = provider.GetRequiredService<MessageProcessor>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

            await Handle(messageProcessor, messageTypes, tokenProvider, httpClientFactory).ConfigureAwait(false);
        }

        /*public static async Task MainSignalR()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddDomainCore()
                .AddJson()
                .Services
                .BuildServiceProvider();

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var jsonConnectionFactory = provider.GetRequiredService<IJsonConnectionFactory>();

            Console.WriteLine("Press enter to connect.");
            Console.ReadLine();

            var hub = new HubConnectionBuilder()
                .WithUrl("http://127.0.0.1:30100/hub")
                .Build();

            var notificator = new Notificator();
            hub.On<JsonSerializedMessage>("Send", message => notificator.NotifyReceived(message));

            await hub.StartAsync(default).ConfigureAwait(false);

            using var sendLock = new SemaphoreSlimLock();
            using var receiveLock = new SemaphoreSlimLock();
            var connection = new SignalRMessageSender(hub).WithNotificator(notificator)
                .WithJson(jsonConnectionFactory)
                .WithLocking(sendLock, receiveLock);

            await Handle(connection, messageTypes).ConfigureAwait(false);
        }*/

        private static async Task Handle(
            MessageProcessor processor,
            IEnumerable<Type> messageTypes,
            IProfileTokenProvider tokenProvider,
            IHttpClientFactory httpClientFactory)
        {
            processor.Subscribe<object>(message =>
            {
                Console.WriteLine($"Received {message.GetType().Name}: {JsonSerializer.Serialize(message, options: new JsonSerializerOptions { WriteIndented = true })}");
                return default;
            });

            Console.WriteLine("messages - show list of available messages");
            Console.WriteLine("<some-message> - initiate process of sending message to server");
            Console.WriteLine("exit - quit the application");

            Console.Write("Character name: ");
            var characterName = Console.ReadLine();
            if (string.IsNullOrEmpty(characterName))
                throw new InvalidOperationException("Character name cannot be empty.");

            var characterId = await CreateCharacterAsync(httpClientFactory, tokenProvider, characterName)
                .ConfigureAwait(false);

            using var cts = new CancellationTokenSource();
            await processor.ConnectAsync(characterId, cts.Token).ConfigureAwait(false);

            while (true)
            {
                var input = Console.ReadLine();
                if (input == null)
                    continue;

                if (input == "exit")
                    return;

                if (input == "messages")
                {
                    Console.WriteLine(JsonSerializer.Serialize(messageTypes.Select(t => t.Name).ToList()));
                    continue;
                }

                int? bulk = null;
                if (input == "bulk")
                {
                    Console.Write("How much: ");
                    bulk = Convert.ToInt32(Console.ReadLine());

                    input = Console.ReadLine();
                    if (input == null)
                        continue;
                }

                var messageType = messageTypes.FirstOrDefault(t => t.Name.ToUpperInvariant() == input.ToUpperInvariant());
                if (messageType == null)
                {
                    Console.WriteLine("Unknown message type.");
                    continue;
                }

                var properties = messageType.GetProperties();
                var message = Activator.CreateInstance(messageType);
                if (message == null)
                    throw new InvalidOperationException($"Could not create instance of {messageType.Name}.");

                foreach (var property in properties)
                {
                    Console.Write($"[{property.PropertyType.Name}] {property.Name} = ");
                    var value = Console.ReadLine() ?? throw new InvalidOperationException("Value is not supplied.");

                    // A hack to support int for now.
                    property.SetValue(message, property.PropertyType == typeof(string) ? (object)value : (object)Convert.ToInt32(value));
                }

                var json = JsonSerializer.Serialize(message);
                Console.WriteLine($"Sending json? (write 'no' to abort)" + bulk == null ? "" : $" BULK: {bulk} times");
                Console.WriteLine(json);

                if (Console.ReadLine() == "no")
                    continue;

                if (bulk == null)
                {
                    await processor.SendWithHandledAcknowledgementAsync(message, default).ConfigureAwait(false);
                    continue;
                }

                for (var i = 1; i <= bulk; i++)
                {
                    await processor.SendWithHandledAcknowledgementAsync(message, default).ConfigureAwait(false);
                }
            }
        }

        private static async ValueTask<string> CreateCharacterAsync(
            IHttpClientFactory httpClientFactory,
            IProfileTokenProvider tokenProvider,
            string characterName)
        {
            var token = await tokenProvider.SignInAsync(default).ConfigureAwait(false);
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using var content = new StringContent(JsonSerializer.Serialize(new
            {
                name = characterName
            }), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("http://127.0.0.1:30103/api/characters", content).ConfigureAwait(false);

            var character = JsonSerializer.Deserialize<Character>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            if (character == null || character.characterId == null)
                throw new InvalidOperationException("Character deserialization failed.");

            return character.characterId;
        }
    }
}
