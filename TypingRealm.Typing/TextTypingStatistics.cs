namespace TypingRealm.Typing
{
    public sealed class TextTypingStatistics
    {
        public TextTypingStatistics(decimal speedCpm)
        {
            // TODO: Validate.

            SpeedCpm = speedCpm;
        }

        public decimal SpeedCpm { get; }
        public decimal SpeedWpm => SpeedCpm / 5;
    }
}
