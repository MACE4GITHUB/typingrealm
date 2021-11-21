using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public sealed record KeyPair(
        string? FromKey, // If it's null - then it's the first character of the string.
        string ToKey,
        decimal Delay);

    public sealed record TextAnalysisResult(
        decimal SpeedCpm,
        IEnumerable<KeyPair> SuccessKeyPairs,
        IEnumerable<KeyPair> ErrorKeyPairs);

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
            var totalTimeMs = events.Last().AbsoluteDelay;
            var speedCpm = textValue.Length / (totalTimeMs / 60000);

            // TODO: Validate shift keys and caps keys: caps keys can only be pressed within shift key window.
            // TODO: If I'm going to accept both Delay and AbsoluteDelay, make sure they are validated towards each other.

            // Statistical data.
            var successKeyPairs = new List<KeyPair>();
            var errorKeyPairs = new List<KeyPair>();

            var handled = new List<KeyPressEvent>();
            var index = 0;
            var errors = textValue.Select(character => false).ToList();

            decimal lastAbsoluteDelay = 0;
            KeyPressEvent? previousKeyPressEvent = null;
            foreach (var @event in events)
            {
                var delay = @event.AbsoluteDelay - previousKeyPressEvent?.AbsoluteDelay ?? 0;

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

                            // TODO: Think about accounting for backspaces before the first character.
                            errorKeyPairs.Add(new KeyPair(null, @event.Key, delay));
                            previousKeyPressEvent = @event;
                        }
                        else
                        {
                            // TODO: Think about accounting for backspaces before the first character.
                            successKeyPairs.Add(new KeyPair(null, @event.Key, delay));
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
                        errors[index] = true;

                        if (!errors[index - 1])
                        {
                            errorKeyPairs.Add(new KeyPair(previousKeyPressEvent.Key, @event.Key, delay));
                            previousKeyPressEvent = @event;
                        }
                    }
                    else
                    {
                        if (!errors[index - 1])
                        {
                            successKeyPairs.Add(new KeyPair(previousKeyPressEvent.Key, @event.Key, delay));
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
                            previousKeyPressEvent = @event;
                            successKeyPairs.Add(new KeyPair(previousKeyPressEvent.Key, @event.Key, delay));
                        }
                        else
                        {
                            previousKeyPressEvent = @event;
                            successKeyPairs.Add(new KeyPair(previousKeyPressEvent.Key, @event.Key, delay));
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
                successKeyPairs,
                errorKeyPairs);
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
}
