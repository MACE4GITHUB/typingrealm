using System;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.Output
{
    public sealed class ConsoleOutput : IOutput
    {
        public void Clear()
        {
            Console.Clear();
        }

        public void WriteLine()
        {
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
    }
}
