using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public sealed record TypingReport(
        IDictionary<string, List<decimal>> KeyDelays,
        IEnumerable<KeyValuePair<string, decimal>> averageKeyDelays,
        IEnumerable<KeyValuePair<string, decimal>> maxKeyDelays,
        IEnumerable<KeyValuePair<string, decimal>> minKeyDelays);

    public interface ITypingReportGenerator
    {
        ValueTask<TypingReport> GenerateReportAsync(string userId);
    }

    public sealed class TypingReportGenerator : ITypingReportGenerator
    {
        private readonly IUserSessionRepository _userSessionRepository;

        public TypingReportGenerator(IUserSessionRepository userSessionRepository)
        {
            _userSessionRepository = userSessionRepository;
        }

        public async ValueTask<TypingReport> GenerateReportAsync(string userId)
        {
            var keyDelays = new Dictionary<string, List<decimal>>();

            await foreach (var userSession in _userSessionRepository.FindAllForUser(userId))
            {
                foreach (var textTypingResult in userSession.GetTextTypingResults())
                {
                    foreach (var @event in textTypingResult.Events.Skip(1))
                    {
                        if (!keyDelays.ContainsKey(@event.Key))
                            keyDelays.Add(@event.Key, new List<decimal>());

                        keyDelays[@event.Key].Add(@event.Delay);
                    }
                }
            }

            var averageKeyDelays = keyDelays.ToDictionary(
                x => x.Key,
                x => x.Value.Average())
                .OrderBy(x => x.Value);

            var shortestKeyDelays = keyDelays.ToDictionary(
                x => x.Key,
                x => x.Value.Min())
                .OrderBy(x => x.Value);

            var longestKeyDelays = keyDelays.ToDictionary(
                x => x.Key,
                x => x.Value.Max())
                .OrderBy(x => x.Value);

            return new TypingReport(keyDelays, averageKeyDelays, shortestKeyDelays, longestKeyDelays);
        }
    }
}
