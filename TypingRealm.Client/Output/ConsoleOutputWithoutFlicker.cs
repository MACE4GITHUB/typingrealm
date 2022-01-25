using System;

namespace TypingRealm.Client.Output;

public sealed class ConsoleOutputWithoutFlicker : ConsoleOutput
{
    private readonly int _screenWidth = 120;
    private readonly int _screenHeight = 30;

    public override void Clear()
    {
        Console.SetCursorPosition(0, 0);
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public override void WriteLine()
    {
        var (Left, _) = Console.GetCursorPosition();
        if (Left < _screenWidth)
            Console.Write(new string(' ', _screenWidth - Left));

        Console.WriteLine();
    }

    public override void FinalizeScreen()
    {
        WriteLine();

        var (_, Top) = Console.GetCursorPosition();

        for (var i = 0; i < _screenHeight - Top; i++)
        {
            WriteLine();
        }
    }
}
