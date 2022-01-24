using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public sealed record TypingReport(
        TextAnalysisResult Result,
        IEnumerable<KeyPairAggregatedData> AggregatedData);

    public interface ITypingReportGenerator
    {
        ValueTask<TypingReport> GenerateReportAsync(string userId, string language, TextGenerationType textGenerationType);
        ValueTask<TypingReport> GenerateReportForUserSessionAsync(string userSessionId, TextGenerationType textGenerationType);

        // Fun little method with experimentation.
        ValueTask<string> GenerateStandardHumanReadableReportAsync(string userId, string language);

        // Fast cached user statistics.
        ValueTask<UserTypingStatistics> GenerateUserStatisticsAsync(string userId, string language, TextGenerationType textGenerationType);
        ValueTask<UserTypingStatistics> GenerateStandardUserStatisticsAsync(string userId, string language);
    }

    public sealed record KeyPairAggregatedData(
        string? FromKey,
        string ToKey,
        decimal AverageDelay,
        decimal MinDelay,
        decimal MaxDelay,
        int SuccessfullyTyped,
        int MadeMistakes)
    {
        public double MistakesToSuccessRatio => SuccessfullyTyped == 0 ? 0 : (double)MadeMistakes / SuccessfullyTyped;
    }

    public sealed class TypingReportGenerator : ITypingReportGenerator
    {
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly ITypingSessionRepository _typingSessionRepository;
        private readonly ITextTypingResultValidator _textTypingResultValidator;
        private readonly IUserTypingStatisticsStore _userTypingStatisticsStore;
        private readonly ITextRepository _textRepository;

        public TypingReportGenerator(
            IUserSessionRepository userSessionRepository,
            ITypingSessionRepository typingSessionRepository,
            ITextTypingResultValidator textTypingResultValidator,
            IUserTypingStatisticsStore userTypingStatisticsStore,
            ITextRepository textRepository)
        {
            _userSessionRepository = userSessionRepository;
            _typingSessionRepository = typingSessionRepository;
            _textTypingResultValidator = textTypingResultValidator;
            _userTypingStatisticsStore = userTypingStatisticsStore;
            _textRepository = textRepository;
        }

        public async ValueTask<UserTypingStatistics> GenerateUserStatisticsAsync(string userId, string language, TextGenerationType textGenerationType)
        {
            var existingStatistics = await _userTypingStatisticsStore.GetUserTypingStatisticsAsync(userId, language, textGenerationType)
                .ConfigureAwait(false);

            var userSessions = Enumerable.Empty<UserSession>();
            userSessions = existingStatistics != null
                ? await _userSessionRepository.FindAllForUserFromTypingResultsAsync(userId, existingStatistics.LastHandledResultUtc)
                    .ConfigureAwait(false)
                : await _userSessionRepository.FindAllForUserAsync(userId)
                    .ConfigureAwait(false);

            var results = new List<TextAnalysisResult>();
            var textsTypedCount = 0;
            DateTime lastHandledResultUtc = default;
            foreach (var userSession in userSessions)
            {
                var typingSession = await _typingSessionRepository.FindAsync(userSession.TypingSessionId)
                    .ConfigureAwait(false);
                if (typingSession == null)
                    throw new InvalidOperationException("Typing session is not found.");

                foreach (var textTypingResult in userSession.GetTextTypingResults())
                {
                    textsTypedCount++;
                    if (textTypingResult.SubmittedResultsUtc > lastHandledResultUtc)
                        lastHandledResultUtc = textTypingResult.SubmittedResultsUtc;

                    var text = typingSession.GetTypingSessionTextAtIndexOrDefault(textTypingResult.TypingSessionTextIndex);
                    if (text == null)
                        throw new InvalidOperationException("Text is not found in typing session.");

                    var textEntity = await _textRepository.FindAsync(text.TextId)
                        .ConfigureAwait(false);
                    if (textEntity == null)
                        throw new InvalidOperationException("Text is not found.");

                    if (textEntity.Language != language)
                        continue;

                    if (textEntity.TextGenerationType != textGenerationType)
                        continue; // Generate statistics only for requested text generation type.

                    var textAnalysisResult = await _textTypingResultValidator.ValidateAsync(text.Value, textTypingResult)
                        .ConfigureAwait(false);

                    results.Add(textAnalysisResult);
                }
            }

            if (results.Count == 0)
            {
                if (existingStatistics != null)
                {
                    // No new data yet.
                    return existingStatistics;
                }

                return new UserTypingStatistics(0, 0, Enumerable.Empty<KeyPairAggregatedData>(), DateTime.UtcNow);
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

            var result = MergeAndReturnNew(existingStatistics, new TypingReport(aggregatedResult, aggregatedData), textsTypedCount, lastHandledResultUtc);
            await _userTypingStatisticsStore.SaveAsync(userId, result, language, textGenerationType)
                .ConfigureAwait(false);

            return result;
        }

        private UserTypingStatistics MergeAndReturnNew(UserTypingStatistics? to, TypingReport from, long textsTypedCount, DateTime lastHandledResultUtc)
        {
            if (to != null)
            {
                var newTextsTypedCount = to.TextsTypedCount + textsTypedCount;

                var newSpeedCpm = (newTextsTypedCount == 0 ? 0 : to.SpeedCpm * ((decimal)to.TextsTypedCount / newTextsTypedCount)) + (newTextsTypedCount == 0 ? 0 : from.Result.SpeedCpm * ((decimal)textsTypedCount / newTextsTypedCount));

                var newKeyPairs = MergeKeyPairs(to.KeyPairs, from.AggregatedData);

                return new UserTypingStatistics(newTextsTypedCount, newSpeedCpm, newKeyPairs, lastHandledResultUtc);
            }

            return new UserTypingStatistics(textsTypedCount, from.Result.SpeedCpm, from.AggregatedData, lastHandledResultUtc);
        }

        private IEnumerable<KeyPairAggregatedData> MergeKeyPairs(
            IEnumerable<KeyPairAggregatedData> to,
            IEnumerable<KeyPairAggregatedData> from)
        {
            var dict = to.ToDictionary(x => $"{x.FromKey}-{x.ToKey}");
            foreach (var keyPair in from)
            {
                if (dict.ContainsKey($"{keyPair.FromKey}-{keyPair.ToKey}"))
                {
                    var existing = dict[$"{keyPair.FromKey}-{keyPair.ToKey}"];
                    var newSuccessfullyTyped = existing.SuccessfullyTyped + keyPair.SuccessfullyTyped;

                    var newKeyPair = new KeyPairAggregatedData(
                        keyPair.FromKey, keyPair.ToKey,
                        newSuccessfullyTyped == 0 ? 0 : (existing.AverageDelay * ((decimal)existing.SuccessfullyTyped / newSuccessfullyTyped)) + (keyPair.AverageDelay * ((decimal)keyPair.SuccessfullyTyped / newSuccessfullyTyped)),
                        newSuccessfullyTyped == 0 ? 0 : (existing.MinDelay * ((decimal)existing.SuccessfullyTyped / newSuccessfullyTyped)) + (keyPair.MinDelay * ((decimal)keyPair.SuccessfullyTyped / newSuccessfullyTyped)),
                        newSuccessfullyTyped == 0 ? 0 : (existing.MaxDelay * ((decimal)existing.SuccessfullyTyped / newSuccessfullyTyped)) + (keyPair.MaxDelay * ((decimal)keyPair.SuccessfullyTyped / newSuccessfullyTyped)),
                        newSuccessfullyTyped,
                        existing.MadeMistakes + keyPair.MadeMistakes);

                    dict[$"{keyPair.FromKey}-{keyPair.ToKey}"] = newKeyPair;
                    continue;
                }

                dict.Add($"{keyPair.FromKey}-{keyPair.ToKey}", keyPair);
            }

            return dict.Values;
        }

        public async ValueTask<TypingReport> GenerateReportAsync(string userId, string language, TextGenerationType textGenerationType)
        {
            var results = new List<TextAnalysisResult>();

            foreach (var userSession in await _userSessionRepository.FindAllForUserAsync(userId).ConfigureAwait(false))
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

                    var textEntity = await _textRepository.FindAsync(text.TextId)
                        .ConfigureAwait(false);
                    if (textEntity == null)
                        throw new InvalidOperationException("Text is not found.");

                    if (textEntity.Language != language || textGenerationType != textEntity.TextGenerationType)
                        continue;

                    var textAnalysisResult = await _textTypingResultValidator.ValidateAsync(text.Value, textTypingResult)
                        .ConfigureAwait(false);

                    results.Add(textAnalysisResult);
                }
            }

            if (results.Count == 0)
            {
                // No data yet.
                return new TypingReport(new TextAnalysisResult(0, Enumerable.Empty<KeyPair>()), Enumerable.Empty<KeyPairAggregatedData>());
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

        public async ValueTask<TypingReport> GenerateReportForUserSessionAsync(string userSessionId, TextGenerationType textGenerationType)
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

                var textEntity = await _textRepository.FindAsync(text.TextId)
                    .ConfigureAwait(false);
                if (textEntity == null)
                    throw new InvalidOperationException("Text is not found.");

                if (textGenerationType != textEntity.TextGenerationType)
                    continue;

                var textAnalysisResult = await _textTypingResultValidator.ValidateAsync(text.Value, textTypingResult)
                    .ConfigureAwait(false);

                results.Add(textAnalysisResult);
            }

            if (results.Count == 0)
            {
                // No data yet.
                return new TypingReport(new TextAnalysisResult(0, Enumerable.Empty<KeyPair>()), Enumerable.Empty<KeyPairAggregatedData>());
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

        public async ValueTask<UserTypingStatistics> GenerateStandardUserStatisticsAsync(string userId, string language)
        {
            var standardStatistics = await GenerateUserStatisticsAsync(userId, language, TextGenerationType.GeneratedStardardText)
                .ConfigureAwait(false);

            var selfImprovementStatistics = await GenerateUserStatisticsAsync(userId, language, TextGenerationType.GeneratedSelfImprovementText)
                .ConfigureAwait(false);

            var statistics = standardStatistics.Merge(selfImprovementStatistics);

            return statistics;
        }

        public async ValueTask<string> GenerateStandardHumanReadableReportAsync(string userId, string language)
        {
            var standardStatistics = await GenerateUserStatisticsAsync(userId, language, TextGenerationType.GeneratedStardardText)
                .ConfigureAwait(false);

            var selfImprovementStatistics = await GenerateUserStatisticsAsync(userId, language, TextGenerationType.GeneratedSelfImprovementText)
                .ConfigureAwait(false);

            var statistics = standardStatistics.Merge(selfImprovementStatistics);

            var builder = new StringBuilder();
            builder.AppendLine($"Your average speed throughout all the time: {statistics.SpeedCpm.ToString("0.###")} CPM ({(statistics.SpeedCpm / 5).ToString("0.###")}).");
            builder.Append("You've made most mistakes when typing ");
            builder.Append(string.Join(", ", statistics.KeyPairs
                .Where(x => x.MadeMistakes > 0)
                .OrderByDescending(x => x.MadeMistakes)
                .Take(4)
                .Select(x => $"['{x.FromKey}' -> '{x.ToKey}'] ({x.MadeMistakes})")));
            builder.AppendLine();

            builder.Append("Your worst mistake-to-correct ratio key pairs are ");
            builder.Append(string.Join(", ", statistics.KeyPairs
                .Where(x => x.MistakesToSuccessRatio > 0)
                .OrderByDescending(x => x.MistakesToSuccessRatio)
                .Take(4)
                .Select(x => $"['{x.FromKey}' -> '{x.ToKey}'] ({x.MistakesToSuccessRatio.ToString("0.###")})")));
            builder.AppendLine();

            builder.Append("Your slowest lowercase key pairs are (with average times)");
            builder.Append(string.Join(", ", statistics.KeyPairs
                .Where(x => x.AverageDelay > 0)
                .Where(x => (x.ToKey.Length == 1 && char.IsLower(x.ToKey[0])) || x.ToKey.Length > 1)
                .OrderByDescending(x => x.AverageDelay)
                .Take(4)
                .Select(x => $"['{x.FromKey}' -> '{x.ToKey}'] ({x.AverageDelay.ToString("0.###")} ms)")));
            builder.AppendLine();

            return builder.ToString();
        }
    }
}
