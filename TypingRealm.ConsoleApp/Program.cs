using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.ConsoleClient;
using TypingRealm.Client;
using TypingRealm.Client.CharacterCreation;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.MainMenu;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;
using TypingRealm.Client.World;
using TypingRealm.Hosting.ConsoleClient;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.Profiles.Api.Resources.Data;

namespace TypingRealm.ConsoleApp
{
    public static class Program
    {
        public static async Task Main()
        {
            string? localProfile = null;
            if (DebugHelpers.UseDevelopmentAuthentication)
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

                if (localProfile != null && DebugHelpers.UseDevelopmentAuthentication)
                    services.AddLocalProfileTokenProvider(localProfile);

                // For every singleton screen handler - it's own typer pool.
                services.AddTransient<ITyperPool, UniqueTyperPool>();

                services.AddSingleton<IOutput, ConsoleOutputWithoutFlicker>();
                services.AddSingleton<ITextGenerator, TextGenerator>();

                services.AddSingleton<ScreenFactory>();
                services.AddTransient<IScreenFactory>(p => p.GetRequiredService<ScreenFactory>());
                services.AddTransient<IScreenNavigation>(p => p.GetRequiredService<ScreenFactory>().ScreenNavigation);

                services.AddSingleton<DialogScreenHandler>();
                services.AddSingleton<IScreenProvider>(p => (IScreenProvider)p.GetRequiredService<IScreenNavigation>());
                services.AddSingleton<IConnectionManager, ConnectionManager>();

                services.AddTransient<IPrinter<MainMenuState>, MainMenuPrinter>();
                services.AddTransient<IPrinter<CharacterCreationState>, CharacterCreationPrinter>();
                services.AddTransient<IPrinter<WorldScreenState>, WorldPrinter>();

                services.AddSingleton<IScreenFactory, ScreenFactory>();
            }).Build();

            await host.StartAsync(cancellationToken)
                .ConfigureAwait(false);

            {
                // Create a couple of characters.
                var charactersClient = host.Services.GetRequiredService<ICharactersClient>();
                await charactersClient.CreateAsync(
                    new CreateCharacterDto("my character"),
                    default).ConfigureAwait(false);
                await charactersClient.CreateAsync(
                    new CreateCharacterDto("my character 2"),
                    default).ConfigureAwait(false);
            }

            RunApplication(
                host.Services.GetRequiredService<IScreenNavigation>(),
                host.Services.GetRequiredService<IScreenProvider>(),
                cancellationToken);

            await host.StopAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public static void RunApplication(
            IScreenNavigation screenNavigation,
            IScreenProvider screenProvider,
            CancellationToken cancellationToken)
        {
            screenNavigation.ScreenObservable.Subscribe(screen =>
            {
                // TODO: Refactor to IObservable<IChangeDetector>.
                var cd = screenProvider.GetCurrentScreen()?.ChangeDetector;
                cd?.NotifyChanged();
            });

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (screenNavigation.Screen == GameScreen.Exit)
                    return;

                var screen = screenProvider.GetCurrentScreen();
                if (screen == null)
                    continue;

                var inputHandler = screen.InputHandler;
                var cd = screen.ChangeDetector;

                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        inputHandler.Escape();
                        break;
                    case ConsoleKey.Backspace:
                        inputHandler.Backspace();
                        break;
                    case ConsoleKey.Tab:
                        inputHandler.Tab();
                        break;
                    default:
                        inputHandler.Type(key.KeyChar);
                        break;
                }

                // A hack to redraw the screen every time a key is pressed (to highlight Typers).
                cd.NotifyChanged();
            }
        }
    }
}
