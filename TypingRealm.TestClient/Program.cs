using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Combat.Messages;
using TypingRealm.Domain;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;
using TypingRealm.RopeWar;

namespace TypingRealm.TestClient
{
    public static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("=== TypingRealm test client ===");
            Console.Write("Type of connection (s - SignalR, cs - Combat SignalR, rw - RopeWar, default - TCP): ");

            await (Console.ReadLine() switch
            {
                "s" => MainSignalR(),
                "cs" => CombatSignalR(),
                "rw" => RopeWarSignalR(),
                "rwt" => RopeWarTcp(),
                _ => MainTpc()
            }).ConfigureAwait(false);
        }

        public static async Task MainTpc()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddDomainCore()
                .AddJson()
                .Services
                .AddProtobuf()
                .BuildServiceProvider();

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var protobufConnectionFactory = provider.GetRequiredService<IProtobufConnectionFactory>();

            Console.WriteLine("Press enter to connect.");
            Console.ReadLine();

            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 30100).ConfigureAwait(false);

            var jsonConnectionFactory = provider.GetRequiredService<IJsonConnectionFactory>();

            using var stream = client.GetStream();
            using var sendLock = new SemaphoreSlimLock();
            using var receiveLock = new SemaphoreSlimLock();
            var connection = protobufConnectionFactory.CreateProtobufConnection(stream)
                .WithJson(jsonConnectionFactory)
                .WithLocking(sendLock, receiveLock);

            await Handle(connection, messageTypes).ConfigureAwait(false);
        }

        public static async Task RopeWarTcp()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddMessageTypesFromAssembly(typeof(JoinContest).Assembly)
                .AddJson()
                .Services
                .AddProtobuf()
                .BuildServiceProvider();

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var protobufConnectionFactory = provider.GetRequiredService<IProtobufConnectionFactory>();

            Console.WriteLine("Press enter to connect.");
            Console.ReadLine();

            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 30102).ConfigureAwait(false);

            var jsonConnectionFactory = provider.GetRequiredService<IJsonConnectionFactory>();

            using var stream = client.GetStream();
            using var sendLock = new SemaphoreSlimLock();
            using var receiveLock = new SemaphoreSlimLock();
            var connection = protobufConnectionFactory.CreateProtobufConnection(stream)
                .WithJson(jsonConnectionFactory)
                .WithLocking(sendLock, receiveLock);

            await Handle(connection, messageTypes).ConfigureAwait(false);
        }

        public static async Task RopeWarSignalR()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddMessageTypesFromAssembly(typeof(JoinContest).Assembly)
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

            Console.Write("Access token: ");
            var accessToken = Console.ReadLine();

            var hub = new HubConnectionBuilder()
                .WithUrl($"http://localhost:30102/hub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
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
        }

        public static async Task CombatSignalR()
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
                .WithUrl("http://localhost:30101/hub")
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

        }

        public static async Task MainSignalR()
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
                .WithUrl("http://localhost:30100/hub")
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
        }

        private static async Task Handle(IConnection connection, IEnumerable<Type> messageTypes)
        {
            _ = ListenForMessagesFromServer(connection);

            Console.WriteLine("Connected.");
            Console.WriteLine("messages - show list of available messages");
            Console.WriteLine("<some-message> - initiate process of sending message to server");
            Console.WriteLine("exit - quit the application");

            while (true)
            {
                var input = Console.ReadLine();
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
                    var value = Console.ReadLine();

                    // A hack to support int for now.
                    property.SetValue(message, property.PropertyType == typeof(string) ? (object)value : (object)Convert.ToInt32(value));
                }

                var json = JsonSerializer.Serialize(message);
                Console.WriteLine($"Sending json? (write 'no' to abort)");
                Console.WriteLine(json);

                if (Console.ReadLine() == "no")
                    continue;

                await connection.SendAsync(message, default).ConfigureAwait(false);
            }
        }

        private static async Task ListenForMessagesFromServer(IConnection connection)
        {
            while (true)
            {
                var message = await connection.ReceiveAsync(default).ConfigureAwait(false);

                switch (message)
                {
                    case Announce say:
                        Console.WriteLine($"Announcement: {say.Message}");
                        break;
                    case Disconnected disconnected:
                        Console.WriteLine($"Disconnected with reason: {disconnected.Reason}");
                        return; // Return after server tells us that he's disconnecting us. Or socket exception will be thrown on the next WaitAsync operation.
                    default:
                        Console.WriteLine($"Received {message.GetType()} message:");
                        var json = JsonSerializer.Serialize(message, options: new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        Console.WriteLine(json);
                        break;
                }
            }
        }
    }
}
