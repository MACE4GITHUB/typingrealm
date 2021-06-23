using System;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.Output
{
    public sealed class ConsoleOutputWithoutFlicker : IOutput
    {
        private readonly int _screenWidth = 120;
        private readonly int _screenHeight = 30;

        public ConsoleOutputWithoutFlicker()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Clear()
        {
            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void WriteLine()
        {
            var (Left, _) = Console.GetCursorPosition();
            if (Left < _screenWidth)
                Console.Write(new string(' ', _screenWidth - Left));

            Console.WriteLine();
        }

        public void Write(string value)
        {
            Console.Write(value);
        }

        public void Write(ITyperInformation typerInformation)
        {
            var currentFg = Console.ForegroundColor;
            var currentBg = Console.BackgroundColor;

            // Highlight already typed part.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.Write(typerInformation.Typed);

            // Highlight mistakes.
            Console.BackgroundColor = currentBg;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(typerInformation.Error);

            // Highlight the rest of text if typer is focused.
            Console.ForegroundColor = typerInformation.IsStartedTyping ? ConsoleColor.DarkYellow : currentFg;
            Console.Write(typerInformation.NotTyped);

            // Revert console color to default.
            Console.ForegroundColor = currentFg;
        }

        public void FinalizeScreen()
        {
            WriteLine();

            var (_, Top) = Console.GetCursorPosition();

            for (var i = 0; i < _screenHeight - Top; i++)
            {
                WriteLine();
            }
        }
    }
}
