using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication;
using TypingRealm.Authentication.ConsoleClient;
using TypingRealm.Client.CharacterCreation;
using TypingRealm.Client.Data;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.MainMenu;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;
using TypingRealm.Communication;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.SignalR;
using TypingRealm.SignalR.Client;
using TypingRealm.World;

namespace TypingRealm.ConsoleApp
{
    public sealed class TextGenerator : ITextGenerator
    {
        private readonly IEnumerator<string> _wordEnumerator;

        public TextGenerator()
        {
            _wordEnumerator = GetWords().GetEnumerator();
        }

        public string GetPhrase()
        {
            _wordEnumerator.MoveNext();

            return _wordEnumerator.Current;
        }

        private IEnumerable<string> GetWords()
        {
            var words = @"Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc"
                .Split(' ');

            foreach (var word in words)
            {
                yield return word;
            }
        }
    }

    public sealed class CharacterService : ICharacterService
    {
        public string GetCharacterName(string characterId)
        {
            return $"ivan name - {characterId}";
        }
    }

    public interface IScreenHandlerProvider
    {
        IScreenHandler GetCurrentScreenHandler();
    }

    public static class Program
    {
        public static async Task Main()
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
                //.AddLogging()
                .AddSerializationCore()
                .AddTyrAuthenticationMessages()
                .AddWorldMessages()
                .Services
                .AddJson()
                .AddProtobufMessageSerializer() // Serialize messages with Protobuf instead of JSON.
                .RegisterClientMessaging() // TODO: Use RegisterClientMessagingBase instead.
                .AddSignalRConnectionFactory()

                .AddCommunication()
                .AddProfileApiClients()
                .RegisterClientConnectionFactoryFactory<SignalRClientConnectionFactoryFactory>();

            services.AddSingleton<IOutput, ConsoleOutput>();
            services.AddSingleton<ICharacterService, CharacterService>();
            services.AddSingleton<ITextGenerator, TextGenerator>();

            services.AddSingleton<DialogScreenHandler>();
            services.AddSingleton<IScreenNavigation, ScreenNavigation>();
            services.AddSingleton<IConnectionManager, ConnectionManager>();

            services.AddSingleton<IMainMenuHandler, MainMenuHandler>();
            services.AddSingleton<IPrinter<MainMenuPrinter.State>, MainMenuPrinter>();
            services.AddSingleton<MainMenuScreenHandler>();

            services.AddSingleton<ICharacterCreationHandler, CharacterCreationHandler>();
            services.AddSingleton<IPrinter<CharacterCreationPrintableState>, CharacterCreationPrinter>();
            services.AddSingleton<CharacterCreationScreenHandler>();

            services.AddSingleton<IDictionary<GameScreen, IScreenHandler>>(p => new Dictionary<GameScreen, IScreenHandler>
            {
                [GameScreen.MainMenu] = p.GetRequiredService<MainMenuScreenHandler>(),
                [GameScreen.CharacterCreation] = p.GetRequiredService<CharacterCreationScreenHandler>()
            });
            services.AddSingleton<ScreenHandlerProvider>();

            var provider = services.BuildServiceProvider();

            // Authenticate in background.
            _ = provider.GetRequiredService<IProfileTokenProvider>().SignInAsync(default);

            var charactersClient = provider.GetRequiredService<ICharactersClient>();
            var belongs = await charactersClient.BelongsToCurrentProfileAsync("id", default)
                .ConfigureAwait(false);

            // Authenticated and got list of characters from API.
            var characters = new[] { "1", "2", "ivan-id" };
            provider.GetRequiredService<MainMenuScreenHandler>()
                .UpdateState(new MainMenuState(characters));

            var screenProvider = provider.GetRequiredService<ScreenHandlerProvider>();

            while (true)
            {
                Console.Clear();
                var screen = screenProvider.GetCurrentScreenHandler();

                screen.PrintState();

                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        screen.Escape();
                        break;
                    case ConsoleKey.Backspace:
                        screen.Backspace();
                        break;
                    case ConsoleKey.Tab:
                        screen.Tab();
                        break;
                    default:
                        screen.Type(key.KeyChar);
                        break;
                }
            }
        }
    }
}
