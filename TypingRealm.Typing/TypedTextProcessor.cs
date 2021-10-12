using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Data.Resources.Typing;

namespace TypingRealm.Typing
{
    public interface ITypedTextProcessor
    {
        Text ProcessTypedText(TypedTextDto dto);
    }

    public sealed class TypedTextProcessor : ITypedTextProcessor
    {
        private readonly ITextValidator _textValidator;

        public TypedTextProcessor(ITextValidator textValidator)
        {
            _textValidator = textValidator;
        }

        public Text ProcessTypedText(TypedTextDto dto)
        {
            var newTextId = Guid.NewGuid();
            var submitDate = DateTime.UtcNow;

            var events = FindDelays(dto.Events).ToList();
            var totalTimeMs = dto.Events.Last().Perf;

            var text = new Text(newTextId, dto.TextData.Text, totalTimeMs, dto.StartedTypingAt, submitDate, events);

            _textValidator.Validate(text);

            return text;
        }

        public static IEnumerable<KeyPressEvent> FindDelays(IEnumerable<KeyPressEventDto> events)
        {
            var isFirstEvent = true;
            decimal previousPerf = 0;

            foreach (var @event in events)
            {
                if (@event.Perf == 0)
                {
                    if (isFirstEvent)
                    {
                        isFirstEvent = false;
                        yield return new KeyPressEvent(@event.Index, @event.Key, @event.Perf);
                        continue;
                    }

                    throw new InvalidOperationException("Cannot have non-first event with zero delay.");
                }

                var currentEventDelay = @event.Perf - previousPerf;

                if (currentEventDelay <= 0)
                    throw new InvalidOperationException("Cannot have zero or negative delays.");

                previousPerf = @event.Perf;

                yield return new KeyPressEvent(@event.Index, @event.Key, currentEventDelay);
            }
        }
    }
}
