using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Typing
{
    /// <summary>
    /// Cached entity used to not get all the typing sessions from the database.
    /// </summary>
    public sealed record UserTypingStatistics(
        long TextsTypedCount,
        decimal SpeedCpm,
        IEnumerable<KeyPairAggregatedData> KeyPairs,
        DateTime LastHandledResultUtc)
    {
        public UserTypingStatistics Merge(UserTypingStatistics selfImprovementStatistics)
        {
            if (TextsTypedCount == 0)
                return selfImprovementStatistics;

            if (selfImprovementStatistics.TextsTypedCount == 0)
                return this;

            return new UserTypingStatistics(
                TextsTypedCount + selfImprovementStatistics.TextsTypedCount,
                (SpeedCpm + selfImprovementStatistics.SpeedCpm) / 2,
                Merge(KeyPairs, selfImprovementStatistics.KeyPairs),
                LastHandledResultUtc > selfImprovementStatistics.LastHandledResultUtc
                    ? LastHandledResultUtc
                    : selfImprovementStatistics.LastHandledResultUtc);
        }

        private static IEnumerable<KeyPairAggregatedData> Merge(
            IEnumerable<KeyPairAggregatedData> left,
            IEnumerable<KeyPairAggregatedData> right)
        {
            var rightDict = right.ToDictionary(x => $"{x.FromKey}->{x.ToKey}");

            foreach (var keyPair in left)
            {
                if (!rightDict.ContainsKey($"{keyPair.FromKey}->{keyPair.ToKey}"))
                {
                    yield return keyPair;
                    continue;
                }

                var rightKeyPair = rightDict[$"{keyPair.FromKey}->{keyPair.ToKey}"];

                // TODO: Fix division by zero here ASAP.
                var successfullyTyped = keyPair.SuccessfullyTyped + rightKeyPair.SuccessfullyTyped;
                if (successfullyTyped == 0)
                    successfullyTyped = 1; // Avoid division by zero.

                yield return new KeyPairAggregatedData(
                    keyPair.FromKey, keyPair.ToKey,
                    // TODO: Recheck math here.
                    ((keyPair.AverageDelay * keyPair.SuccessfullyTyped) + (rightKeyPair.AverageDelay * rightKeyPair.SuccessfullyTyped)) / successfullyTyped,
                    ((keyPair.MinDelay * keyPair.SuccessfullyTyped) + (rightKeyPair.MinDelay * rightKeyPair.SuccessfullyTyped)) / successfullyTyped,
                    ((keyPair.MaxDelay * keyPair.SuccessfullyTyped) + (rightKeyPair.MaxDelay * rightKeyPair.SuccessfullyTyped)) / successfullyTyped,
                    keyPair.SuccessfullyTyped + rightKeyPair.SuccessfullyTyped,
                    keyPair.MadeMistakes + rightKeyPair.MadeMistakes);
            }
        }
    }
}
