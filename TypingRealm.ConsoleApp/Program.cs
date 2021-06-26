using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.ConsoleClient;
using TypingRealm.Client.CharacterCreation;
using TypingRealm.Client.Data;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.MainMenu;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;
using TypingRealm.Client.World;
using TypingRealm.Hosting;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.Profiles.Api.Resources.Data;

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

    public sealed class StubCharacterService : ICharacterService
    {
        public string GetCharacterName(string characterId)
        {
            return $"ivan name - {characterId}";
        }
    }

    public static class Program
    {
        public static async Task Main()
        {
            string? localProfile = null;
            if (DebugHelpers.UseLocalAuthentication)
            {
                Console.Write("Profile for local token: ");
                localProfile = Console.ReadLine() ?? "default";
            }

            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.CursorVisible = false;

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Canceling...");
                cts.Cancel();
                e.Cancel = true;
            };

            var cancellationToken = cts.Token;

            using var host = HostFactory.CreateConsoleAppHostBuilder(messageTypes =>
            {
                var services = messageTypes.Services;

                if (localProfile != null && DebugHelpers.UseLocalAuthentication)
                    services.AddLocalProfileTokenProvider(localProfile);

                // For every singleton screen handler - it's own typer pool.
                services.AddTransient<ITyperPool, UniqueTyperPool>();

                services.AddSingleton<IOutput, ConsoleOutputWithoutFlicker>();
                services.AddSingleton<ICharacterService, StubCharacterService>();
                services.AddSingleton<ILocationService, StubLocationService>();
                services.AddSingleton<ITextGenerator, TextGenerator>();

                services.AddSingleton<DialogScreenHandler>();
                services.AddSingleton<IScreenNavigation, ScreenNavigation>();
                services.AddSingleton<IConnectionManager, ConnectionManager>();

                services.AddSingleton(p =>
                {
                    var typerPool = p.GetRequiredService<ITyperPool>();
                    var componentPool = new ComponentPool(typerPool);
                    var mainMenuModel = new MainMenuState(typerPool, componentPool);

                    var mainMenuScreenStateManager = new MainMenuScreenStateManager(
                        p.GetRequiredService<ICharactersClient>(),
                        mainMenuModel);

                    var mainMenuInputHandler = new MainMenuInputHandler(
                        typerPool,
                        componentPool,
                        mainMenuModel,
                        p.GetRequiredService<IScreenNavigation>(),
                        p.GetRequiredService<IConnectionManager>());
                    var mainMenuPrinter = new MainMenuPrinter(
                        p.GetRequiredService<IOutput>());

                    return new ScreenDependencies<MainMenuScreenStateManager, MainMenuPrinter, MainMenuInputHandler>(
                        mainMenuScreenStateManager,
                        mainMenuPrinter,
                        mainMenuInputHandler);
                });

                services.AddTransient(p =>
                {
                    var dependencies = p.GetRequiredService<ScreenDependencies<MainMenuScreenStateManager, MainMenuPrinter, MainMenuInputHandler>>();
                    return dependencies.Handler;
                });
                services.AddTransient(p =>
                {
                    var dependencies = p.GetRequiredService<ScreenDependencies<MainMenuScreenStateManager, MainMenuPrinter, MainMenuInputHandler>>();
                    return dependencies.Manager;
                });
                services.AddTransient<IPrinter<MainMenuState>>(p =>
                {
                    var dependencies = p.GetRequiredService<ScreenDependencies<MainMenuScreenStateManager, MainMenuPrinter, MainMenuInputHandler>>();
                    return dependencies.Printer;
                });

                services.AddSingleton(p =>
                {
                    var typerPool = p.GetRequiredService<ITyperPool>();

                    var worldScreenStateManager = new WorldScreenStateManager(
                        typerPool,
                        p.GetRequiredService<IConnectionManager>());
                    var worldInputHandler = new WorldInputHandler(
                        typerPool,
                        p.GetRequiredService<IScreenNavigation>(),
                        p.GetRequiredService<IConnectionManager>());
                    var worldPrinter = new WorldPrinter(
                        p.GetRequiredService<IOutput>(),
                        typerPool);

                    return new ScreenDependencies<WorldScreenStateManager, WorldPrinter, WorldInputHandler>(
                        worldScreenStateManager,
                        worldPrinter,
                        worldInputHandler);
                });

                services.AddTransient(p =>
                {
                    var dependencies = p.GetRequiredService<ScreenDependencies<WorldScreenStateManager, WorldPrinter, WorldInputHandler>>();
                    return dependencies.Handler;
                });
                services.AddTransient(p =>
                {
                    var dependencies = p.GetRequiredService<ScreenDependencies<WorldScreenStateManager, WorldPrinter, WorldInputHandler>>();
                    return dependencies.Manager;
                });
                services.AddTransient<IPrinter<WorldScreenState>>(p =>
                {
                    var dependencies = p.GetRequiredService<ScreenDependencies<WorldScreenStateManager, WorldPrinter, WorldInputHandler>>();
                    return dependencies.Printer;
                });

                services.AddSingleton(p =>
                {
                    var typerPool = p.GetRequiredService<ITyperPool>();

                    var characterCreationScreenStateManager = new CharacterCreationScreenStateManager(typerPool);
                    var characterCreationInputHandler = new CharacterCreationInputHandler(
                        typerPool,
                        p.GetRequiredService<IScreenNavigation>(),
                        p.GetRequiredService<ICharactersClient>());
                    var characterCreationPrinter = new CharacterCreationPrinter(
                        p.GetRequiredService<IOutput>(),
                        typerPool);

                    return new ScreenDependencies<CharacterCreationScreenStateManager, CharacterCreationPrinter, CharacterCreationInputHandler>(
                        characterCreationScreenStateManager,
                        characterCreationPrinter,
                        characterCreationInputHandler);
                });

                services.AddTransient(p =>
                {
                    var dependencies = p.GetRequiredService<ScreenDependencies<CharacterCreationScreenStateManager, CharacterCreationPrinter, CharacterCreationInputHandler>>();
                    return dependencies.Handler;
                });
                services.AddTransient(p =>
                {
                    var dependencies = p.GetRequiredService<ScreenDependencies<CharacterCreationScreenStateManager, CharacterCreationPrinter, CharacterCreationInputHandler>>();
                    return dependencies.Manager;
                });
                services.AddTransient<IPrinter<CharacterCreationScreenState>>(p =>
                {
                    var dependencies = p.GetRequiredService<ScreenDependencies<CharacterCreationScreenStateManager, CharacterCreationPrinter, CharacterCreationInputHandler>>();
                    return dependencies.Printer;
                });

                services.AddSingleton<IDictionary<GameScreen, IInputHandler>>(p => new Dictionary<GameScreen, IInputHandler>
                {
                    [GameScreen.MainMenu] = p.GetRequiredService<MainMenuInputHandler>(),
                    [GameScreen.CharacterCreation] = p.GetRequiredService<CharacterCreationInputHandler>(),
                    [GameScreen.World] = p.GetRequiredService<WorldInputHandler>()
                });
                services.AddSingleton<IDictionary<GameScreen, IChangeDetector>>(p => new Dictionary<GameScreen, IChangeDetector>
                {
                    [GameScreen.MainMenu] = p.GetRequiredService<MainMenuScreenStateManager>(),
                    [GameScreen.CharacterCreation] = p.GetRequiredService<CharacterCreationScreenStateManager>(),
                    [GameScreen.World] = p.GetRequiredService<WorldScreenStateManager>()
                });
                services.AddSingleton<IScreenProvider, InputHandlerProvider>();

                services.AddSingleton(
                    p => p.GetRequiredService<MainMenuScreenStateManager>().StateObservable);
                services.AddSingleton(
                    p => p.GetRequiredService<CharacterCreationScreenStateManager>().StateObservable);
                services.AddSingleton(
                    p => p.GetRequiredService<WorldScreenStateManager>().StateObservable);

                // TODO: Create them per page and dispose accordingly.
                services.AddSingleton(
                    p => new StatePrinter<MainMenuState>(
                        p.GetRequiredService<IScreenNavigation>(),
                        p.GetRequiredService<IOutput>(),
                        p.GetRequiredService<IObservable<MainMenuState>>(),
                        p.GetRequiredService<IPrinter<MainMenuState>>(),
                        GameScreen.MainMenu));
                services.AddSingleton(
                    p => new StatePrinter<CharacterCreationScreenState>(
                        p.GetRequiredService<IScreenNavigation>(),
                        p.GetRequiredService<IOutput>(),
                        p.GetRequiredService<IObservable<CharacterCreationScreenState>>(),
                        p.GetRequiredService<IPrinter<CharacterCreationScreenState>>(),
                        GameScreen.CharacterCreation));
                services.AddSingleton(
                    p => new StatePrinter<WorldScreenState>(
                        p.GetRequiredService<IScreenNavigation>(),
                        p.GetRequiredService<IOutput>(),
                        p.GetRequiredService<IObservable<WorldScreenState>>(),
                        p.GetRequiredService<IPrinter<WorldScreenState>>(),
                        GameScreen.World));
            }).Build();

            await host.StartAsync(cancellationToken)
                .ConfigureAwait(false);

            {
                // Create a couple of characters.
                var charactersClient = host.Services.GetRequiredService<ICharactersClient>();
                await charactersClient.CreateAsync(new CreateCharacterDto
                {
                    Name = "my character"
                }, default).ConfigureAwait(false);
                await charactersClient.CreateAsync(new CreateCharacterDto
                {
                    Name = "my character 2"
                }, default).ConfigureAwait(false);
            }

            // Initialize singletons.
            // TODO: Create them per page and dispose accordingly.
            host.Services.GetRequiredService<StatePrinter<MainMenuState>>();
            host.Services.GetRequiredService<StatePrinter<CharacterCreationScreenState>>();
            host.Services.GetRequiredService<StatePrinter<WorldScreenState>>();

            RunApplication(
                host.Services.GetRequiredService<IScreenNavigation>(),
                host.Services.GetRequiredService<IScreenProvider>());

            await host.StopAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public static void RunApplication(
            IScreenNavigation screenNavigation,
            IScreenProvider screenProvider)
        {
            screenNavigation.ScreenObservable.Subscribe(screen =>
            {
                // TODO: Refactor to IObservable<IChangeDetector>.
                var cd = screenProvider.GetCurrentChangeDetector();
                cd.NotifyChanged();
            });

            while (true)
            {
                if (screenNavigation.Screen == GameScreen.Exit)
                    return;

                var screen = screenProvider.GetCurrentInputHandler();
                var cd = screenProvider.GetCurrentChangeDetector();

                //output.Clear();
                //screen.PrintState();
                //output.FinalizeScreen();

                var key = Console.ReadKey(true);
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

                // A hack to redraw the screen every time a key is pressed (to highlight Typers).
                cd.NotifyChanged();
            }
        }
    }
}
