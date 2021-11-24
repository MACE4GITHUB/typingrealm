using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public sealed record TypingReport(
        TextAnalysisResult Result,
        IEnumerable<KeyPairAggregatedData> AggregatedData);

    public interface ITypingReportGenerator
    {
        ValueTask<TypingReport> GenerateReportAsync(string userId);
        ValueTask<TypingReport> GenerateReportForUserSessionAsync(string userSessionId);
    }

    public sealed record KeyPairAggregatedData(
        string? FromKey,
        string ToKey,
        decimal AverageDelay,
        decimal MinDelay,
        decimal MaxDelay,
        int SuccessfullyTyped,
        int MadeMistakes);

    public sealed class TypingReportGenerator : ITypingReportGenerator
    {
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly ITypingSessionRepository _typingSessionRepository;
        private readonly ITextTypingResultValidator _textTypingResultValidator;

        public TypingReportGenerator(
            IUserSessionRepository userSessionRepository,
            ITypingSessionRepository typingSessionRepository,
            ITextTypingResultValidator textTypingResultValidator)
        {
            _userSessionRepository = userSessionRepository;
            _typingSessionRepository = typingSessionRepository;
            _textTypingResultValidator = textTypingResultValidator;
        }

        public async ValueTask<TypingReport> GenerateReportAsync(string userId)
        {
            var results = new List<TextAnalysisResult>();

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

                    var textAnalysisResult = await _textTypingResultValidator.ValidateAsync(text.Value, textTypingResult)
                        .ConfigureAwait(false);

                    results.Add(textAnalysisResult);
                }
            }

            var aggregatedResult = new TextAnalysisResult(
                results.Sum(x => x.SpeedCpm) / results.Count,
                results.SelectMany(x => x.KeyPairs));

            var specificKeys = aggregatedResult.KeyPairs.GroupBy(x => new { x.FromKey, x.ShouldBeKey });
            var aggregatedData = specificKeys.Select(x => new KeyPairAggregatedData(
                x.Key.FromKey, x.Key.ShouldBeKey,
                x.Where(y => y.Type == KeyPairType.Correct).Any()
                    ? x.Where(y => y.Type == KeyPairType.Correct).Average(y => y.Delay)
                    : 0,
                x.Where(y => y.Type == KeyPairType.Correct).Any()
                    ? x.Where(y => y.Type == KeyPairType.Correct).Min(y => y.Delay)
                    : 0,
                x.Where(y => y.Type == KeyPairType.Correct).Any()
                    ? x.Where(y => y.Type == KeyPairType.Correct).Max(y => y.Delay)
                    : 0,
                x.Count(y => y.Type == KeyPairType.Correct),
                x.Count(y => y.Type == KeyPairType.Mistake)));

            return new TypingReport(aggregatedResult, aggregatedData);
        }

        public async ValueTask<TypingReport> GenerateReportForUserSessionAsync(string userSessionId)
        {
            var results = new List<TextAnalysisResult>();

            var userSession = await _userSessionRepository.FindAsync(userSessionId)
                .ConfigureAwait(false);
            if (userSession == null)
                throw new InvalidOperationException("Could not find user session.");

            var typingSession = await _typingSessionRepository.FindAsync(userSession.TypingSessionId)
                .ConfigureAwait(false);
            if (typingSession == null)
                throw new InvalidOperationException("Typing session is not found.");

            foreach (var textTypingResult in userSession.GetTextTypingResults())
            {
                var text = typingSession.GetTypingSessionTextAtIndexOrDefault(textTypingResult.TypingSessionTextIndex);
                if (text == null)
                    throw new InvalidOperationException("Text is not found in typing session.");

                var textAnalysisResult = await _textTypingResultValidator.ValidateAsync(text.Value, textTypingResult)
                    .ConfigureAwait(false);

                results.Add(textAnalysisResult);
            }

            var aggregatedResult = new TextAnalysisResult(
                results.Sum(x => x.SpeedCpm) / results.Count,
                results.SelectMany(x => x.KeyPairs));

            var specificKeys = aggregatedResult.KeyPairs.GroupBy(x => new { x.FromKey, x.ShouldBeKey });
            var aggregatedData = specificKeys.Select(x => new KeyPairAggregatedData(
                x.Key.FromKey, x.Key.ShouldBeKey,
                x.Where(y => y.Type == KeyPairType.Correct).Any()
                    ? x.Where(y => y.Type == KeyPairType.Correct).Average(y => y.Delay)
                    : 0,
                x.Where(y => y.Type == KeyPairType.Correct).Any()
                    ? x.Where(y => y.Type == KeyPairType.Correct).Min(y => y.Delay)
                    : 0,
                x.Where(y => y.Type == KeyPairType.Correct).Any()
                    ? x.Where(y => y.Type == KeyPairType.Correct).Max(y => y.Delay)
                    : 0,
                x.Count(y => y.Type == KeyPairType.Correct),
                x.Count(y => y.Type == KeyPairType.Mistake)));

            return new TypingReport(aggregatedResult, aggregatedData);
        }
    }
}
