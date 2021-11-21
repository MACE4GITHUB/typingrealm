using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public sealed record TypingReport(
        IDictionary<string, List<KeyDelayData>> KeyDelays,
        IEnumerable<KeyValuePair<string, KeyDelayData>> averageKeyDelays,
        IEnumerable<KeyValuePair<string, KeyDelayData>> maxKeyDelays,
        IEnumerable<KeyValuePair<string, KeyDelayData>> minKeyDelays);

    public interface ITypingReportGenerator
    {
        ValueTask<TypingReport> GenerateReportAsync(string userId);
    }

    public sealed record KeyDelayData(
        decimal Delay,
        string KeyPair);

    public sealed class TypingReportGenerator : ITypingReportGenerator
    {
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly ITypingSessionRepository _typingSessionRepository;

        public TypingReportGenerator(
            IUserSessionRepository userSessionRepository,
            ITypingSessionRepository typingSessionRepository)
        {
            _userSessionRepository = userSessionRepository;
            _typingSessionRepository = typingSessionRepository;
        }

        public async ValueTask<TypingReport> GenerateReportAsync(string userId)
        {
            var keyDelays = new Dictionary<string, List<KeyDelayData>>();
            var keyErrors = new Dictionary<string, List<KeyDelayData>>();

            await foreach (var userSession in _userSessionRepository.FindAllForUser(userId))
            {
                var typingSession = await _typingSessionRepository.FindAsync(userSession.TypingSessionId)
                    .ConfigureAwait(false);
                if (typingSession == null)
                    throw new InvalidOperationException("Typing session is not found.");

                foreach (var textTypingResult in userSession.GetTextTypingResults())
                {
                    var text = typingSession.GetTypingSessionTextAtIndexOrDefault(textTypingResult.TypingSessionTextIndex);
                    if (text == null)
                        throw new InvalidOperationException("Text is not found in typing session.");

                    var previousEvent = textTypingResult.Events.First();
                    foreach (var @event in textTypingResult.Events.Skip(1))
                    {
                        if (@event.KeyAction != KeyAction.Press || @event.Key == "shift")
                            continue;

                        if (@event.Key.Length == 1 /* character */ && @event.Key != text.Value[@event.Index].ToString())
                        {
                            // Wrong key. Do not include in delays, include in errors.
                            if (!keyErrors.ContainsKey(@event.Key))
                                keyErrors.Add(@event.Key, new List<KeyDelayData>());

                            keyErrors[@event.Key].Add(new KeyDelayData(@event.AbsoluteDelay - previousEvent.AbsoluteDelay, $"{previousEvent.Key} -> {@event.Key}"));
                            previousEvent = @event;

                            continue;
                        }

                        if (!keyDelays.ContainsKey(@event.Key))
                            keyDelays.Add(@event.Key, new List<KeyDelayData>());

                        keyDelays[@event.Key].Add(new KeyDelayData(@event.AbsoluteDelay - previousEvent.AbsoluteDelay, $"{previousEvent.Key} -> {@event.Key}"));
                        previousEvent = @event;
                    }
                }
            }

            foreach (var item in keyDelays)
            {
                var list = item.Value.ToList();

                item.Value.Clear();
                item.Value.AddRange(list.OrderBy(x => x.Delay));
            }

            var averageKeyDelays = keyDelays.ToDictionary(
                x => x.Key,
                x => new KeyDelayData(x.Value.Average(y => y.Delay), ""))
                .OrderBy(x => x.Value.Delay)
                .ToList();

            var shortestKeyDelays = keyDelays.ToDictionary(
                x => x.Key,
                x => x.Value.OrderBy(y => y.Delay).First())
                .OrderBy(x => x.Value.Delay)
                .ToList();

            var longestKeyDelays = keyDelays.ToDictionary(
                x => x.Key,
                x => x.Value.OrderByDescending(y => y.Delay).First())
                .OrderBy(x => x.Value.Delay)
                .ToList();

            return new TypingReport(keyDelays, averageKeyDelays, longestKeyDelays, shortestKeyDelays);
        }
    }
}
