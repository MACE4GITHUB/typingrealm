using System;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.Output;

public class ConsoleOutput : IOutput
{
    public ConsoleOutput()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public virtual void Clear()
    {
        Console.Clear();
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public virtual void WriteLine()
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

    public virtual void FinalizeScreen()
    {
        // Do nothing.
    }

    public void Write(IInputComponent inputComponent)
    {
        var currentFg = Console.ForegroundColor;

        if (inputComponent.IsFocused)
            Console.ForegroundColor = ConsoleColor.Yellow;

        Console.Write(" ");
        if (inputComponent.IsFocused)
            Console.Write("**");
        Console.Write("[ ");
        Console.Write(inputComponent.Value);
        Console.Write(" ]");
        if (inputComponent.IsFocused)
            Console.Write("**");

        // Revert console color to default.
        Console.ForegroundColor = currentFg;

        if (!inputComponent.IsFocused)
        {
            Console.Write("   < ");
            Write(inputComponent.FocusTyper);
        }
    }
}
