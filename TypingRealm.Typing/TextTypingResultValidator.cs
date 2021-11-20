using System;
using System.Linq;
using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public interface ITextTypingResultValidator
    {
        ValueTask ValidateAsync(string textValue, TextTypingResult textTypingResult);
    }

    public sealed class TextTypingResultValidator : ITextTypingResultValidator
    {
        private const int MaxDelayMs = 10000;

        public ValueTask ValidateAsync(string textValue, TextTypingResult textTypingResult)
        {
            // TODO: Validate shift keys and caps keys: caps keys can only be pressed within shift key window.
            // TODO: If I'm going to accept both Delay and AbsoluteDelay, make sure they are validated towards each other.

            KeyPressEvent? previousEvent = null;
            var index = 0;
            var errors = textValue.Select(character => false).ToList();

            foreach (var @event in textTypingResult.Events)
            {
                if (previousEvent == null)
                {
                    if (@event.Key == "backspace")
                        throw new InvalidOperationException("Cannot have backspace as first symbol.");

                    previousEvent = @event;

                    if (@event.Delay != 0)
                        throw new InvalidOperationException("First event should have 0 delay.");

                    if (@event.Index != 0)
                        throw new InvalidOperationException("First event's index should be 0.");

                    if (@event.Key == "shift" || @event.KeyAction == KeyAction.Release)
                        continue;

                    if (textValue[index].ToString() != @event.Key)
                        errors[index] = true;

                    index++;
                    continue;
                }

                if (@event.Delay <= 0)
                    throw new InvalidOperationException("Event's Delay should have positive value.");

                // TODO: Allow zero delay if it's pressing different keys or pressing and releasing different keys simultaneously.
                // Do not validate delays but validate AbsoluteDelays. Do not accept Delays from the frontend, it will send only Absolute delays.

                if (@event.Delay > MaxDelayMs)
                    throw new InvalidOperationException("Event's Delay is too big.");

                if (index != @event.Index)
                    throw new InvalidOperationException("Sequence of keyboard events is corrupted: invalid order.");

                if (@event.Key == "shift" || @event.KeyAction == KeyAction.Release)
                    continue;

                switch (@event.Key)
                {
                    case "backspace":
                        if (index > 0)
                        {
                            index--;
                            errors[index] = false;
                        }
                        break;
                    default:
                        if (textValue[index].ToString() != @event.Key)
                            errors[index] = true;
                        index++;
                        break;
                }
            }

            if (index != textValue.Length)
                throw new InvalidOperationException("Did not finish typing text to the end.");

            if (errors.Any(error => error))
            {
                throw new InvalidOperationException("Text is not typed till the end without errors.");
                // TODO: Text was not typed completely without errors - do not count it to statistics.
            }

            // Validation is successful, the whole text has been typed.
            return default;
        }
    }
}
