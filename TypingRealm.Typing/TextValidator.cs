using System;

namespace TypingRealm.Typing
{
    public interface ITextValidator
    {
        void Validate(Text text);
    }

    public sealed class TextValidator : ITextValidator
    {
        private static readonly int _maxDelaySeconds = 10000;

        public void Validate(Text text)
        {
            KeyPressEvent? previousEvent = null;
            var nextSupposedIndex = 0;

            foreach (var @event in text.Events)
            {
                if (nextSupposedIndex == text.Value.Length)
                {
                    // We typed the whole text, but there were more events.
                    throw new InvalidOperationException("Some events were trailing after the whole text has been typed.");
                }

                if (previousEvent == null)
                {
                    if (@event.Key == "backspace")
                        throw new InvalidOperationException("Cannot have backspace as first symbol.");

                    previousEvent = @event;

                    if (previousEvent.Delay != 0)
                        throw new InvalidOperationException("First event should have 0 delay.");

                    if (previousEvent.Index != 0)
                        throw new InvalidOperationException("First event's index should be 0.");

                    nextSupposedIndex++;
                    continue;
                }

                if (@event.Delay <= 0)
                    throw new InvalidOperationException("Event's Delay should have positive value.");

                if (@event.Delay > _maxDelaySeconds)
                    throw new InvalidOperationException("Event's Delay is too big.");

                if (nextSupposedIndex != @event.Index)
                    throw new InvalidOperationException("Sequence of keyboard events is corrupted: invalid order.");

                switch (@event.Key)
                {
                    case "backspace":
                        if (nextSupposedIndex > 0)
                            nextSupposedIndex--;
                        break;
                    default:
                        nextSupposedIndex++;
                        break;
                }
            }

            if (nextSupposedIndex == text.Value.Length)
            {
                // Validation is successful, the whole text has been typed.
                return;
            }
        }
    }
}
