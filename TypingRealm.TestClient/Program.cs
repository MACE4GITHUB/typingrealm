using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.RopeWar;
using TypingRealm.World;

namespace TypingRealm.TestClient
{
    public enum AuthenticationProviderType
    {
        Local = 1,
        Auth0 = 2,
        IdentityServer = 3
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProfileTokenProvider(this IServiceCollection services, AuthenticationProviderType authenticationProviderType, string profile = "ivan")
        {
            switch (authenticationProviderType)
            {
                case AuthenticationProviderType.Local:
                    services.AddTransient<IProfileTokenProvider>(
                        _ => new LocalProfileTokenProvider(profile));
                    break;
                case AuthenticationProviderType.Auth0:
                    var auth0Config = new Auth0AuthenticationConfiguration();
                    services.AddSingleton<IProfileTokenProvider>(new PkceClient(auth0Config.Issuer, auth0Config.PkceClientId));
                    break;
                case AuthenticationProviderType.IdentityServer:
                    var idsConfig = new IdentityServerAuthenticationConfiguration();
                    services.AddSingleton<IProfileTokenProvider>(new PkceClient(idsConfig.Issuer, idsConfig.PkceClientId));
                    break;
                default:
                    throw new InvalidOperationException("Unknown authentication provider type.");
            }

            return services;
        }
    }

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
            Console.Write("Type of connection (s - SignalR, cs - Combat SignalR, rw - RopeWar, default - TCP): ");

            await (Console.ReadLine() switch
            {
                /*"s" => MainSignalR(),
                "cs" => CombatSignalR(),*/
                "rw" => RopeWarSignalR(),
                "world" => WorldSignalR(),
                /*"rwt" => RopeWarTcp(),
                _ => MainTpc()*/
            }).ConfigureAwait(false);
        }

        /*public static async Task MainTpc()
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
        }*/

        /*public static async Task RopeWarTcp()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddTyrAuthenticationMessages()
                .AddRopeWarMessages()
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
            await client.ConnectAsync("127.0.0.1", 30112).ConfigureAwait(false);

            using var stream = client.GetStream();
            using var sendLock = new SemaphoreSlimLock();
            using var receiveLock = new SemaphoreSlimLock();
            var connection = protobufConnectionFactory.CreateProtobufConnection(stream)
                .WithLocking(sendLock, receiveLock);

            await Handle(connection, messageTypes).ConfigureAwait(false);
        }*/

        public static async Task RopeWarSignalR()
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
                .AddJson()
                .Services
                .AddProfileTokenProvider(authenticationType, profile)
                .BuildServiceProvider();

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var jsonConnectionFactory = provider.GetRequiredService<IJsonConnectionFactory>();

            Console.WriteLine("Press enter to connect.");
            Console.ReadLine();

            var tokenProvider = provider.GetRequiredService<IProfileTokenProvider>();
            var accessToken = await tokenProvider.SignInAsync().ConfigureAwait(false);

            var hub = new HubConnectionBuilder()
                .WithUrl($"http://localhost:30102/hub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .Build();

            var notificator = new Notificator();
            hub.On<JsonSerializedMessage>("Send", message => notificator.NotifyReceived(message));

            await hub.StartAsync(default).ConfigureAwait(false);

            var connection = new SignalRMessageSender(hub).WithNotificator(notificator)
                .WithJson(jsonConnectionFactory);

            await Handle(connection, messageTypes, tokenProvider, null!).ConfigureAwait(false);
        }

        public static async Task WorldSignalR()
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
                .AddJson()
                .Services
                .AddProfileTokenProvider(authenticationType, profile)
                .BuildServiceProvider();

            var messageTypes = provider.GetRequiredService<IMessageTypeCache>()
                .GetAllTypes()
                .Select(idToType => idToType.Value)
                .ToList();

            var jsonConnectionFactory = provider.GetRequiredService<IJsonConnectionFactory>();

            Console.WriteLine("Press enter to connect.");
            Console.ReadLine();

            var tokenProvider = provider.GetRequiredService<IProfileTokenProvider>();
            var accessToken = await tokenProvider.SignInAsync().ConfigureAwait(false);

            var hub = new HubConnectionBuilder()
                .WithUrl($"http://localhost:30111/hub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .Build();

            var notificator = new Notificator();
            hub.On<JsonSerializedMessage>("Send", message => notificator.NotifyReceived(message));

            await hub.StartAsync(default).ConfigureAwait(false);

            var connection = new SignalRMessageSender(hub).WithNotificator(notificator)
                .WithJson(jsonConnectionFactory);

            async ValueTask<IConnection> ConnectToRw()
            {
                var accessToken = await tokenProvider.SignInAsync().ConfigureAwait(false);

                var hub = new HubConnectionBuilder()
                    .WithUrl($"http://localhost:30102/hub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(accessToken);
                    })
                    .Build();

                var notificator = new Notificator();
                hub.On<JsonSerializedMessage>("Send", message => notificator.NotifyReceived(message));

                await hub.StartAsync(default).ConfigureAwait(false);

                return new SignalRMessageSender(hub).WithNotificator(notificator)
                    .WithJson(jsonConnectionFactory);
            }

            await Handle(connection, messageTypes, tokenProvider, ConnectToRw).ConfigureAwait(false);
        }


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
        }*/

        private static async Task Handle(IConnection connection, IEnumerable<Type> messageTypes, IProfileTokenProvider tokenProvider, Func<ValueTask<IConnection>> connectToRw, string? characterId = null)
        {
            _ = ListenForMessagesFromServer(connection, tokenProvider);
            Console.WriteLine("Connected.");
            Console.WriteLine("messages - show list of available messages");
            Console.WriteLine("<some-message> - initiate process of sending message to server");
            Console.WriteLine("exit - quit the application");
            Console.WriteLine("connect-to-rw - connect to ropewar from world");

            Console.WriteLine("Connect automatically and create a character? (y) (uses local token if not authenticated)");

            var token = await tokenProvider.SignInAsync().ConfigureAwait(false);

            string? clientId = null;
            if (Console.ReadLine() == "y")
            {
                Console.WriteLine($"Access token: {token}");

                if (characterId == null)
                {
                    Console.Write("Character name: ");
                    var characterName = Console.ReadLine();

                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    var response = httpClient.PostAsync("http://localhost:30103/api/characters", new StringContent(JsonSerializer.Serialize(new
                    {
                        name = characterName
                    }), Encoding.UTF8, "application/json"));

                    var character = JsonSerializer.Deserialize<Character>(await response.Result.Content.ReadAsStringAsync().ConfigureAwait(false));
                    clientId = character?.characterId;
                }
                else
                {
                    clientId = characterId;
                }

                Console.Write("Group: ");
                var group = Console.ReadLine() ?? throw new InvalidOperationException("Group is not supplied.");

                await _listener!.SendRpcAsync(new Authenticate(token))
                    .ConfigureAwait(false);

                await connection.SendAsync(new Connect(clientId!, group), default)
                    .ConfigureAwait(false);
            }

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

                if (input == "connect-to-rw")
                {
                    await Handle(await connectToRw().ConfigureAwait(false), messageTypes, tokenProvider, connectToRw, clientId).ConfigureAwait(false);
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

                await connection.SendAsync(message, default).ConfigureAwait(false);
            }
        }

        private static MessageListener? _listener;
        private static async Task ListenForMessagesFromServer(IConnection connection, IProfileTokenProvider tokenProvider)
        {
            _listener = new MessageListener(connection);
            _listener.Subscribe<object>(async message =>
            {
                switch (message)
                {
                    case Announce say:
                        Console.WriteLine($"Announcement: {say.Message}");
                        break;
                    case Disconnected disconnected:
                        Console.WriteLine($"Disconnected with reason: {disconnected.Reason}");
                        return; // Return after server tells us that he's disconnecting us. Or socket exception will be thrown on the next WaitAsync operation.
                    case TokenExpired _:
                        Console.WriteLine($"Received TokenExpired message. Re-sending token.");
                        var token = await tokenProvider.SignInAsync().ConfigureAwait(false);
                        await connection.SendAsync(new Authenticate(token), default).ConfigureAwait(false);
                        break;
                    default:
                        Console.WriteLine($"Received {message.GetType()} message:");
                        var json = JsonSerializer.Serialize(message, options: new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        Console.WriteLine(json);
                        break;
                }
            });
        }
    }

    public sealed class MessageListener
    {
        private readonly IConnection _connection;
        private readonly Dictionary<string, Func<object, ValueTask>> _handlers
            = new Dictionary<string, Func<object, ValueTask>>();

        public MessageListener(IConnection connection)
        {
            _connection = connection;
            _ = ListenForMessagesFromServer();
        }

        public async Task ListenForMessagesFromServer()
        {
            while (true)
            {
                var message = await _connection.ReceiveAsync(default).ConfigureAwait(false);

                await AsyncHelpers.WhenAll(_handlers.Values.Select(handler => handler(message)))
                    .ConfigureAwait(false);
            }
        }

        public string Subscribe<TMessage>(Func<TMessage, ValueTask> handler)
        {
            var subscriptionId = Guid.NewGuid().ToString();

            _handlers.Add(subscriptionId, message =>
            {
                if (message is TMessage tMessage)
                    return handler(tMessage);

                return default;
            });

            return subscriptionId;
        }

        public void Unsubscribe(string subscriptionId)
        {
            _handlers.Remove(subscriptionId);
        }

        public async ValueTask SendRpcAsync(Message message)
        {
            var isAcknowledged = false;
            message.MessageId = Guid.NewGuid().ToString();

            ValueTask Handler(AcknowledgeReceived acknowledgeReceived)
            {
                if (acknowledgeReceived.MessageId == message.MessageId)
                    isAcknowledged = true;

                return default;
            }

            var subscriptionId = Subscribe<AcknowledgeReceived>(Handler);

            try
            {
                await _connection.SendAsync(message, default).ConfigureAwait(false);

                var i = 0;
                while (!isAcknowledged)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    i++;

                    if (i > 300)
                        throw new InvalidOperationException("Acknowledgement is not received.");
                }
            }
            finally
            {
                Unsubscribe(subscriptionId);
            }
        }
    }
}
