namespace TypingRealm.Typing
{
    public sealed class TextTypingStatisticsCalculator : ITextTypingStatisticsCalculator
    {
        public TextTypingStatistics Calculate(Text text)
        {
            var textLength = text.Value.Length;
            var timeMs = text.TotalTimeMs;

            return new TextTypingStatistics(textLength / (timeMs / 1000) * 60);
        }
    }
}
