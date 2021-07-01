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
            _wordEnumerator.MoveNext();
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

                services.AddSingleton<ScreenFactory>();
                services.AddTransient<IScreenFactory>(p => p.GetRequiredService<ScreenFactory>());
                services.AddTransient<IScreenNavigation>(p => p.GetRequiredService<ScreenFactory>().ScreenNavigation);

                services.AddSingleton<DialogScreenHandler>();
                services.AddSingleton<IScreenProvider>(p => (IScreenProvider)p.GetRequiredService<IScreenNavigation>());
                services.AddSingleton<IConnectionManager, ConnectionManager>();

                services.AddTransient<IPrinter<MainMenuState>, MainMenuPrinter>();
                services.AddTransient<IPrinter<CharacterCreationState>, CharacterCreationPrinter>();

                services.AddSingleton<IScreenFactory, ScreenFactory>();
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

            var a = host.Services.GetRequiredService<IScreenNavigation>();

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
                var cd = screenProvider.GetCurrentScreen()?.ChangeDetector;
                cd?.NotifyChanged();
            });

            while (true)
            {
                if (screenNavigation.Screen == GameScreen.Exit)
                    return;

                var screen = screenProvider.GetCurrentScreen();
                if (screen == null)
                    continue;

                var inputHandler = screen.InputHandler;
                var cd = screen.ChangeDetector;

                //output.Clear();
                //screen.PrintState();
                //output.FinalizeScreen();

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
