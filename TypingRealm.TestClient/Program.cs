using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Protobuf;

namespace TypingRealm.TestClient
{
    public static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("=== TypingRealm test client ===");

            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddProtobuf()
                .Services
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

            using var stream = client.GetStream();
            var connection = protobufConnectionFactory.CreateProtobufConnection(stream);

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

                    property.SetValue(message, value);
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
                        Console.WriteLine($"Received unknown {message.GetType()} message.");
                        break;
                }
            }
        }
    }
}
