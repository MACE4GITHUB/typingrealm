using System;
using System.Collections.Generic;
using TypingRealm.Client.CharacterCreation;
using TypingRealm.Client.Data;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.MainMenu;
using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

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
        public static void Main()
        {
            // Resolved deps.
            var output = new ConsoleOutput();
            var characterService = new CharacterService();

            var textGenerator = new TextGenerator();
            var dialogModalScreenHandler = new DialogScreenHandler(textGenerator, output);
            var screenNavigation = new ScreenNavigation(dialogModalScreenHandler);
            var connectionManager = new ConnectionManager();
            var mainMenuHandler = new MainMenuHandler(screenNavigation, connectionManager);
            var mainMenuPrinter = new MainMenuPrinter(output, characterService);
            var mainMenuScreenHandler = new MainMenuScreenHandler(textGenerator, mainMenuHandler, mainMenuPrinter);
            var characterCreationHandler = new CharacterCreationHandler(screenNavigation);
            var characterCreationPrinter = new CharacterCreationPrinter(output);
            var characterCreationScreenHandler = new CharacterCreationScreenHandler(textGenerator, characterCreationHandler, characterCreationPrinter);

            var screenProvider = new ScreenHandlerProvider(screenNavigation, new Dictionary<GameScreen, IScreenHandler>
            {
                [GameScreen.MainMenu] = mainMenuScreenHandler,
                [GameScreen.CharacterCreation] = characterCreationScreenHandler
            });

            // Authenticated and got list of characters from API.
            var characters = new[] { "1", "2", "ivan-id" };
            mainMenuScreenHandler.UpdateState(new MainMenuState(characters));

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
