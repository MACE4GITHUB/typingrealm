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

                    if (previousEvent.Delay != 0)
                        throw new InvalidOperationException("First event should have 0 delay.");

                    if (previousEvent.Index != 0)
                        throw new InvalidOperationException("First event's index should be 0.");

                    if (textValue[index].ToString() != @event.Key)
                        errors[index] = true;

                    index++;
                    continue;
                }

                if (@event.Delay <= 0)
                    throw new InvalidOperationException("Event's Delay should have positive value.");

                if (@event.Delay > MaxDelayMs)
                    throw new InvalidOperationException("Event's Delay is too big.");

                if (index != @event.Index)
                    throw new InvalidOperationException("Sequence of keyboard events is corrupted: invalid order.");

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
