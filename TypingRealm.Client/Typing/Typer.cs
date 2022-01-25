using System;

namespace TypingRealm.Client.Typing;

public sealed class Typer : ITyperInformation
{
    private const char NewLine = '\n';
    private const char WhiteSpace = ' ';
    private string _text;

#pragma warning disable CS8618 // The Reset method sets fields.
    public Typer(string text)
#pragma warning restore CS8618
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text should not be empty.", nameof(text));

        Reset(text);
    }

    public Typer(string text, int progress) : this(text)
    {
        if (progress > _text.Length)
            throw new ArgumentException("Progress should be less (or equal) than text length.", nameof(progress));

        Typed = _text.Substring(0, progress);
    }

    /// <summary>
    /// For convenient typers mapping.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Typed { get; private set; }
    public string Error { get; private set; }
    public string NotTyped => _text[Typed.Length..];

    public bool IsStartedTyping => Typed.Length > 0;
    public bool IsFinishedTyping => Typed == _text;

    public void Type(char character)
    {
        if (IsFinishedTyping || Error != string.Empty)
            return;

        var nextCharacter = NotTyped[0];

        if (character == nextCharacter)
        {
            Typed += character;
            return;
        }

        if (nextCharacter == NewLine && character == WhiteSpace)
        {
            Typed += NewLine;
            return;
        }

        Error += character;
    }

    public void Backspace()
    {
        if (Error.Length > 0)
        {
            Error = Error[1..];
            return;
        }

        if (Typed.Length > 0)
            Typed = Typed[0..^1];
    }

    public void Reset(string text)
    {
        _text = text;
        Reset();
    }

    public void Reset()
    {
        Typed = string.Empty;
        Error = string.Empty;
    }
}
