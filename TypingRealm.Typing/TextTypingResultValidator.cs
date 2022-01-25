using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TypingRealm.Typing;

public enum KeyPairType
{
    Unspecified = 0,

    Correct = 1,
    Mistake = 2,
    Correction = 3
}

public sealed record KeyPair(
    string FromKey, // If it's null - then it's the first character of the string.
    string ToKey,
    string ShouldBeKey,
    decimal Delay,
    KeyPairType Type,
    decimal ShiftToKeyDelay);

public sealed record TextAnalysisResult(
    decimal SpeedCpm,
    IEnumerable<KeyPair> KeyPairs);

public interface ITextTypingResultValidator
{
    ValueTask<TextAnalysisResult> ValidateAsync(string textValue, TextTypingResult textTypingResult);
}

public sealed class TextTypingResultValidator : ITextTypingResultValidator
{
    private const int MaxDelayMs = 10000;

    public ValueTask<TextAnalysisResult> ValidateAsync(string textValue, TextTypingResult textTypingResult)
    {
        return new ValueTask<TextAnalysisResult>(
            AnalyzeText(textValue, textTypingResult.Events));
    }

    // TODO: Merge with Validator. Initial validation should generate all the statistical data and save it to some kind of cache / view / store.
    private TextAnalysisResult AnalyzeText(string textValue, IEnumerable<KeyPressEvent> events)
    {
        // TODO: Valitade totalTime here (should be close to the last event absolutedelay).
        var totalTimeMs = events.Last().AbsoluteDelay; // Last event should be the last key press of the text. If it's not - we throw validation error later on.
        var speedCpm = textValue.Length / (totalTimeMs / 60000);

        if (events.Last().Key != textValue[^1].ToString() || events.Last().KeyAction != KeyAction.Press)
            throw new InvalidOperationException("Last event key doesn't match the last character of the text.");

        // TODO: Validate shift keys and caps keys: caps keys can only be pressed within shift key window.
        // TODO: If I'm going to accept both Delay and AbsoluteDelay, make sure they are validated towards each other.

        // Statistical data.
        var successKeyPairs = new List<KeyPair>();

        var handled = new List<KeyPressEvent>();
        var index = 0;
        var errors = textValue.Select(character => false).ToList();

        decimal lastAbsoluteDelay = 0;
        KeyPressEvent? previousKeyPressEvent = null;
        var missedKeyIndex = -1;
        var missedKey = "";
        var uselessBackspace = false;

        foreach (var @event in events)
        {
            if (@event.Index == textValue.Length && !errors.Any(error => error))
                throw new InvalidOperationException("Text has been typing to the end but there are still some events present.");

            var delay = @event.AbsoluteDelay - (previousKeyPressEvent?.AbsoluteDelay ?? 0);

            var shiftToKeyDelay = IsCharacter(@event.Key) && char.IsUpper(@event.Key[0])
                ? @event.AbsoluteDelay - handled.Last(x => x.Key == "shift" && x.KeyAction == KeyAction.Press).AbsoluteDelay
                : 0;

            if (previousKeyPressEvent == null)
            {
                if (@event.Key == "backspace")
                    throw new InvalidOperationException("Cannot have backspace as first symbol.");

                /*if (@event.Delay != 0)
                    throw new InvalidOperationException("First event should have 0 delay.");*/

                if (handled.Count == 0 && @event.AbsoluteDelay != 0)
                    throw new InvalidOperationException("First event should have 0 delay.");

                if (@event.Index != 0)
                    throw new InvalidOperationException("First event's index should be 0.");

                if (IsCharacter(@event.Key))
                {
                    if (textValue[index].ToString() != @event.Key)
                    {
                        errors[index] = true;

                        successKeyPairs.Add(new KeyPair("", @event.Key, textValue[index].ToString(), delay, KeyPairType.Mistake, shiftToKeyDelay));
                        previousKeyPressEvent = @event;
                        missedKeyIndex = @event.Index;
                        missedKey = @event.Key;
                    }
                    else
                    {
                        successKeyPairs.Add(new KeyPair("", @event.Key, @event.Key, delay, KeyPairType.Correct, shiftToKeyDelay));
                        previousKeyPressEvent = @event;
                    }

                    index++;
                }

                handled.Add(@event);
                continue;
            }

            // TODO: Consider not allowing ZERO Delay & AbsoluteDelay equal to previous. Make this check strict.
            /*if (@event.Delay < 0)
                throw new InvalidOperationException("Event's Delay should have positive or zero value.");*/

            if (@event.AbsoluteDelay < handled[^1].AbsoluteDelay)
                throw new InvalidOperationException("Event's AbsoluteDelay should be higher than previous event's AbsoluteDelay.");

            // TODO: This check doesn't work because of fluctuating digits.
            /*if (@event.Delay != @event.AbsoluteDelay - handled[^1].AbsoluteDelay)
                throw new InvalidOperationException("Event's Delay was calculated incorrectly, it doesn't correspond to AbsoluteDelay of previous event.");*/

            if (delay > MaxDelayMs)
                throw new InvalidOperationException("Event's Delay is too big. Cannot be inactive in the middle of the typing.");

            if (index != @event.Index)
                throw new InvalidOperationException("Sequence of keyboard events is corrupted: invalid order.");

            if (@event.KeyAction == KeyAction.Press && IsCharacter(@event.Key))
            {
                if (textValue[index].ToString() != @event.Key)
                {
                    if (index > 0 && !errors[index - 1])
                    {
                        errors[index] = true;

                        if (missedKeyIndex == -1)
                        {
                            var previousKey = uselessBackspace ? "" : previousKeyPressEvent.Key;
                            uselessBackspace = false;

                            successKeyPairs.Add(new KeyPair(previousKey, @event.Key, textValue[index].ToString(), delay, KeyPairType.Mistake, shiftToKeyDelay));
                            previousKeyPressEvent = @event;
                            missedKeyIndex = @event.Index;
                            missedKey = @event.Key;
                        }
                    }
                }
                else
                {
                    var previousKey = uselessBackspace ? "" : previousKeyPressEvent.Key;
                    uselessBackspace = false;

                    if (index > 0 && !errors[index - 1] && missedKeyIndex == -1)
                    {
                        successKeyPairs.Add(new KeyPair(previousKey, @event.Key, @event.Key, delay, KeyPairType.Correct, shiftToKeyDelay));
                        previousKeyPressEvent = @event;
                    }

                    if (index == 0)
                    {
                        successKeyPairs.Add(new KeyPair(previousKey, @event.Key, @event.Key, delay, KeyPairType.Correct, shiftToKeyDelay));
                        previousKeyPressEvent = @event;
                    }
                }

                index++;
            }

            if (@event.KeyAction == KeyAction.Press && IsBackspace(@event.Key))
            {
                if (index > 0)
                {
                    index--;
                    if (errors[index])
                    {
                        errors[index] = false;
                        if (missedKeyIndex == -1 || missedKeyIndex == index)
                        {
                            uselessBackspace = false;
                            successKeyPairs.Add(new KeyPair(missedKey, @event.Key, "", delay, KeyPairType.Correction, 0));
                            previousKeyPressEvent = @event;

                            if (missedKeyIndex == index)
                            {
                                missedKeyIndex = -1;
                                missedKey = "";
                            }
                        }
                    }
                    else
                    {
                        uselessBackspace = true;
                        // Do not log backspace when there are no errors.
                    }
                }
            }

            handled.Add(@event);
            continue;
        }

        if (index != textValue.Length)
            throw new InvalidOperationException("Did not finish typing text to the end.");

        if (errors.Any(error => error))
        {
            throw new InvalidOperationException("Text contains non-corrected errors.");
            // TODO: Text was not typed completely without errors - do not count it to statistics.
        }

        return new TextAnalysisResult(
            speedCpm,
            successKeyPairs);
    }

    private bool IsCharacter(string key)
    {
        return key.Length == 1;
    }

    private bool IsBackspace(string key)
    {
        return key == "backspace";
    }
}
