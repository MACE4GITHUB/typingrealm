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
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;
using TypingRealm.RopeWar;
using TypingRealm.SignalR;
using TypingRealm.SignalR.Client;
using TypingRealm.Tcp.Client;

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
            Console.Write("Type of connection (rw - RopeWar SignalR, rwt - RopeWar TCP / Protobuf): ");

            await (Console.ReadLine() switch
            {
                "rw" => RopeWarSignalR(),
                "rwt" => RopeWarTcp(),
                _ => throw new InvalidOperationException("Invalid type of connection")
            }).ConfigureAwait(false);
        }

        public static async Task RopeWarTcp(bool useLocalAuth = false)
        {
            var services = new ServiceCollection();

            services.AddAuth0ProfileTokenProvider();

            if (useLocalAuth)
            {
                Console.Write("Profile for local token: ");
                var profile = Console.ReadLine() ?? "default";

                services.AddLocalProfileTokenProvider(profile);
            }

            services
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
            _ = await tokenProvider.SignInAsync().ConfigureAwait(false);

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var messageProcessor = provider.GetRequiredService<MessageProcessor>();

            using var cts = new CancellationTokenSource();
            await messageProcessor.ConnectAsync(cts.Token).ConfigureAwait(false);

            await Handle(messageProcessor, messageTypes, tokenProvider).ConfigureAwait(false);
        }

        public static async Task RopeWarSignalR(bool useLocalAuth = false)
        {
            var services = new ServiceCollection();

            services.AddAuth0ProfileTokenProvider();

            if (useLocalAuth)
            {
                Console.Write("Profile for local token: ");
                var profile = Console.ReadLine() ?? "default";

                services.AddLocalProfileTokenProvider(profile);
            }

            services
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
            _ = await tokenProvider.SignInAsync().ConfigureAwait(false);

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var messageProcessor = provider.GetRequiredService<MessageProcessor>();

            using var cts = new CancellationTokenSource();
            await messageProcessor.ConnectAsync(cts.Token).ConfigureAwait(false);

            await Handle(messageProcessor, messageTypes, tokenProvider).ConfigureAwait(false);
        }

        /*public static async Task WorldSignalR()
        {
            Console.Write("Authentication type (a - auth0, i - identityserver, l - local token): ");
            var authenticationType = Console.ReadLine()?.ToLowerInvariant() switch
            {
                "a" => AuthenticationProviderType.Auth0,
                "i" => AuthenticationProviderType.IdentityServer,
                "l" => AuthenticationProviderType.Local,
                _ => throw new InvalidOperationException("Invalid type.")
            };

            var profile = "ivan";
            if (authenticationType == AuthenticationProviderType.Local)
            {
                Console.Write("Profile for local token (default = 'ivan'): ");
                profile = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(profile))
                    profile = "ivan";
            }

            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddTyrAuthenticationMessages()
                .AddRopeWarMessages()
                .AddWorldMessages()
                .Services
                .AddJson()
                .AddProfileTokenProvider(authenticationType, profile)
                .BuildServiceProvider();

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            Console.WriteLine("Press enter to connect.");
            Console.ReadLine();

            var tokenProvider = provider.GetRequiredService<IProfileTokenProvider>();
            var accessToken = await tokenProvider.SignInAsync().ConfigureAwait(false);

            var hub = new HubConnectionBuilder()
                .WithUrl($"http://127.0.0.1:30111/hub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .Build();

            var notificator = new Notificator();
            hub.On<ServerToClientMessageData>("Send", message => notificator.NotifyReceived(message));

            await hub.StartAsync(default).ConfigureAwait(false);

            var connection = new SignalRMessageSender(hub).WithNotificator(notificator);

            async ValueTask<IConnection> ConnectToRw()
            {
                var accessToken = await tokenProvider.SignInAsync().ConfigureAwait(false);

                var hub = new HubConnectionBuilder()
                    .WithUrl($"http://127.0.0.1:30102/hub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(accessToken);
                    })
                    .WithAutomaticReconnect() // TODO: Test if this works as we expect.
                    .Build();

                var notificator = new Notificator();
                hub.On<ServerToClientMessageData>("Send", message => notificator.NotifyReceived(message));

                await hub.StartAsync(default).ConfigureAwait(false);

                return new SignalRMessageSender(hub).WithNotificator(notificator);
            }

            await Handle(connection, messageTypes, tokenProvider, ConnectToRw).ConfigureAwait(false);
        }*/


        /*public static async Task CombatSignalR()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddMessageTypesFromAssembly(typeof(TargetingPlayer).Assembly)
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
                .WithUrl("http://127.0.0.1:30101/hub")
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

        private static async Task Handle(MessageProcessor processor, IEnumerable<Type> messageTypes, IProfileTokenProvider tokenProvider)
        {
            processor.Subscribe<object>(message =>
            {
                Console.WriteLine($"Received {message.GetType().Name}: {JsonSerializer.Serialize(message, options: new JsonSerializerOptions{ WriteIndented = true })}");
                return default;
            });

            Console.WriteLine("messages - show list of available messages");
            Console.WriteLine("<some-message> - initiate process of sending message to server");
            Console.WriteLine("exit - quit the application");

            Console.Write("Character name: ");
            var characterName = Console.ReadLine();

            var token = await tokenProvider.SignInAsync().ConfigureAwait(false);
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using var content = new StringContent(JsonSerializer.Serialize(new
            {
                name = characterName
            }), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("http://127.0.0.1:30103/api/characters", content).ConfigureAwait(false);

            var character = JsonSerializer.Deserialize<Character>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            var clientId = character?.characterId;
            var group = "lobby";

            await processor.SendRpcAsync(new Authenticate(token), default)
                .ConfigureAwait(false);

            await processor.SendAsync(new Connect(clientId!, group), default)
                .ConfigureAwait(false);

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
                Console.WriteLine($"Sending json? (write 'no' to abort)");
                Console.WriteLine(json);

                if (Console.ReadLine() == "no")
                    continue;

                await processor.SendAsync(message, default).ConfigureAwait(false);
            }
        }
    }
}
